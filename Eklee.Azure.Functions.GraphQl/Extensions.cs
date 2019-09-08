using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Eklee.Azure.Functions.GraphQl.Actions;
using Eklee.Azure.Functions.GraphQl.Actions.RequestContextValueExtractors;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Queries;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Azure.Functions.GraphQl.Repository.Http;
using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Azure.Functions.GraphQl.Repository.Search.Filters;
using Eklee.Azure.Functions.GraphQl.Repository.TableStorage;
using Eklee.Azure.Functions.GraphQl.Validations;
using Eklee.Azure.Functions.Http;
using FastMember;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Http;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Types.Relay;
using GraphQL.Types.Relay.DataObjects;
using GraphQL.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class Extensions
	{
		public static void RegisterGraphQl<TSchema>(this ContainerBuilder builder) where TSchema : ISchema
		{
			builder.RegisterType<GraphQlDomain>().As<IGraphQlDomain>().InstancePerLifetimeScope();
			builder.RegisterType<DocumentExecuter>().As<IDocumentExecuter>().SingleInstance();
			builder.RegisterType<DocumentWriter>().As<IDocumentWriter>().SingleInstance();
			builder.RegisterType<TSchema>().As<ISchema>().SingleInstance();
			builder.RegisterType<QueryArgumentsBuilder>().As<IQueryArgumentsBuilder>().SingleInstance();

			builder.RegisterType<DocumentDbComparisonInt>().As<IDocumentDbComparison>().SingleInstance();
			builder.RegisterType<DocumentDbComparisonString>().As<IDocumentDbComparison>().SingleInstance();
			builder.RegisterType<DocumentDbComparisonDate>().As<IDocumentDbComparison>().SingleInstance();
			builder.RegisterType<DocumentDbComparisonBool>().As<IDocumentDbComparison>().SingleInstance();
			builder.RegisterType<DocumentDbComparisonGuid>().As<IDocumentDbComparison>().SingleInstance();

			builder.RegisterType<TableStorageComparisonInt>().As<ITableStorageComparison>().SingleInstance();
			builder.RegisterType<TableStorageComparisonString>().As<ITableStorageComparison>().SingleInstance();
			builder.RegisterType<TableStorageComparisonDate>().As<ITableStorageComparison>().SingleInstance();
			builder.RegisterType<TableStorageComparisonBool>().As<ITableStorageComparison>().SingleInstance();
			builder.RegisterType<TableStorageComparisonGuid>().As<ITableStorageComparison>().SingleInstance();

			builder.RegisterType<InMemoryCompareInt>().As<IInMemoryCompare>().SingleInstance();
			builder.RegisterType<InMemoryCompareString>().As<IInMemoryCompare>().SingleInstance();
			builder.RegisterType<InMemoryCompareDateTime>().As<IInMemoryCompare>().SingleInstance();
			builder.RegisterType<InMemoryComparerProvider>().As<IInMemoryComparerProvider>().SingleInstance();

			builder.RegisterType<SearchFilterModelQueryArgument>().As<IModelMemberQueryArgument>().SingleInstance();
			builder.RegisterType<SearchModelQueryArgument>().As<IModelMemberQueryArgument>().SingleInstance();
			builder.RegisterType<StringQueryArgument>().As<IModelMemberQueryArgument>().SingleInstance();
			builder.RegisterType<DateQueryArgument>().As<IModelMemberQueryArgument>().SingleInstance();
			builder.RegisterType<IntQueryArgument>().As<IModelMemberQueryArgument>().SingleInstance();
			builder.RegisterType<BoolQueryArgument>().As<IModelMemberQueryArgument>().SingleInstance();
			builder.RegisterType<GuidQueryArgument>().As<IModelMemberQueryArgument>().SingleInstance();
			builder.RegisterType<ModelMemberQueryArgumentProvider>().As<IModelMemberQueryArgumentProvider>().SingleInstance();

			builder.RegisterType<ContextValueResolver>().As<IContextValueResolver>().SingleInstance();

			builder.RegisterType<StringSearchFilter>().As<ISearchFilter>().SingleInstance();
			builder.RegisterType<NumericSearchFilter>().As<ISearchFilter>().SingleInstance();
			builder.RegisterType<SearchFilterProvider>().As<ISearchFilterProvider>().SingleInstance();

			builder.RegisterType<GraphDependencyResolver>().As<IDependencyResolver>();

			builder.RegisterType<QueryBuilderFactory>();
			builder.RegisterType<InputBuilderFactory>();
			builder.RegisterGeneric(typeof(ModelConventionInputType<>));
			builder.RegisterGeneric(typeof(ModelConventionType<>));
			builder.RegisterGeneric(typeof(ConnectionType<>));
			builder.RegisterType<PageInfoType>();
			builder.RegisterGeneric(typeof(EdgeType<>));
			builder.RegisterGeneric(typeof(ModelEnumConventionType<>));

			builder.RegisterType<FieldMutationResolver>().As<IFieldMutationResolver>();
			builder.RegisterType<ConnectionEdgeResolver>().As<IConnectionEdgeResolver>();
			builder.RegisterType<ConnectionEdgeHandler>()
				.As<IConnectionEdgeHandler>()
				.As<IMutationPreAction>();

			builder.RegisterType<InMemoryRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<HttpRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<DocumentDbRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<SearchRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<SearchMappedModels>()
				.As<ISearchMappedModels>()
				.As<IMutationPostAction>()
				.SingleInstance();
			builder.RegisterType<TableStorageRepository>().As<IGraphQlRepository>().SingleInstance();

			builder.RegisterType<MutationActionsProvider>().As<IMutationActionsProvider>().SingleInstance();

			builder.RegisterType<GraphQlRepositoryProvider>().As<IGraphQlRepositoryProvider>().SingleInstance();
			builder.RegisterType<GraphRequestContext>().As<IGraphRequestContext>().InstancePerLifetimeScope();

			builder.RegisterType<ModelTransformerProvider>()
				.As<IModelTransformerProvider>()
				.SingleInstance();
		}

		public static void UseSystemModelTransformers(this ContainerBuilder builder)
		{
			builder.RegisterType<AutoIdGenerator>()
				.As<IModelTransformer>()
				.SingleInstance();

			builder.RegisterType<AutoDateTimeGenerator>()
				.As<IModelTransformer>()
				.SingleInstance();
		}

		public static void UseValueFromRequestContextGenerator(this ContainerBuilder builder)
		{
			builder.RegisterType<ValueFromRequestContextGenerator>()
				.As<IModelTransformer>()
				.SingleInstance();
		}

		public static async Task<IActionResult> ProcessGraphQlRequest(this ExecutionContext executionContext, HttpRequest httpRequest)
		{
			var validateionResult = executionContext.ValidateJwt();

			if (validateionResult != null) return validateionResult;

			var logger = executionContext.Resolve<ILogger>();

			if (httpRequest.ContentType == "application/graphql")
			{
				// https://graphql.org/learn/serving-over-http/
				// If the "application/graphql" Content-Type header is present, treat the HTTP POST body contents as the GraphQL query string.
				return await ProcessRequest(executionContext, httpRequest, logger, body => new GraphQlDomainRequest { Query = body });
			}

			if (httpRequest.ContentType == "application/json")
			{
				return await ProcessRequest(executionContext, httpRequest, logger, JsonConvert.DeserializeObject<GraphQlDomainRequest>);
			}

			logger.LogWarning($"{httpRequest.ContentType} is not supported.");
			return new BadRequestResult();
		}

		private static async Task<IActionResult> ProcessRequest(ExecutionContext executionContext, HttpRequest httpRequest, ILogger logger, Func<string, GraphQlDomainRequest> handler)
		{
			var graphQlDomain = executionContext.Resolve<IGraphQlDomain>();

			var requestBody = await httpRequest.ReadAsStringAsync();

			logger.LogInformation($"Request-Body: {requestBody}");

			var results = await graphQlDomain.ExecuteAsync(handler(requestBody));

			if (results.Errors != null && results.Errors.Any(x =>
					x.InnerException != null && x.InnerException.GetType() == typeof(SecurityException)))
				return new UnauthorizedResult();

			return new OkObjectResult(results);
		}

		/// <summary>
		/// Gets Connection.
		/// </summary>
		/// <typeparam name="T">T item.</typeparam>
		/// <param name="context">Connection context.</param>
		/// <param name="enumerable">An IEnumerable of T items.</param>
		/// <param name="defaultPageLimit">Default page limit.</param>
		/// <returns></returns>
		public static async Task<Connection<T>> GetConnectionAsync<T>(this ResolveConnectionContext<object> context, IEnumerable<T> enumerable, int defaultPageLimit = 10)
		{
			// Ref: https://graphql.org/learn/pagination/

			var list = enumerable.ToList();

			var total = list.Count;
			int skip = 0;
			int take = defaultPageLimit;

			var edges = list.Select(TransformToEdge).ToList();

			if (!string.IsNullOrEmpty(context.After))
			{
				skip = GetIndex(context.After) + 1;
			}

			if (context.First.HasValue)
			{
				take = context.First.Value;
			}

			var hasNextPage = total > skip + take;

			var filtered = edges.Skip(skip).Take(take).ToList();

			var first = filtered.FirstOrDefault().GetCursor();

			var connection = new Connection<T>
			{
				TotalCount = total,
				Edges = filtered,
				PageInfo = new PageInfo
				{
					StartCursor = first,
					EndCursor = filtered.LastOrDefault().GetCursor(),
					HasNextPage = hasNextPage,
					HasPreviousPage = GetIndex(first) > 0
				}
			};

			return await Task.FromResult(connection);
		}

		private static string GetCursor<T>(this Edge<T> edge)
		{
			return edge?.Cursor;
		}

		private static Edge<T> TransformToEdge<T>(T item, int index)
		{
			var cursor = Convert.ToBase64String(Encoding.UTF8.GetBytes(index.ToString()));

			return new Edge<T> { Cursor = cursor, Node = item };
		}

		private static int GetIndex(string cursor)
		{
			return !string.IsNullOrEmpty(cursor) ? Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(cursor))) : -1;
		}

		public static string GetDescription(this Member member)
		{
			var description = (DescriptionAttribute)member.GetAttribute(typeof(DescriptionAttribute), false);
			return description != null ? description.Description : "";
		}

		private static SelectValue CreateSelectValue(Field field)
		{
			var value = new SelectValue
			{
				FieldName = field.Name,
				SelectValues = field.SelectionSet.Children.Select(c => (Field)c).Select(CreateSelectValue).ToList()
			};

			return value;
		}

		public static void PopulateSelectValues(this ContextValue contextValue, ResolveFieldContext<object> context)
		{
			contextValue.SelectValues = context.SubFields.Select(x => CreateSelectValue(x.Value)).ToList();
		}

		public static string GetKey<T>(this T item)
		{
			var f = TypeAccessor.Create(typeof(T));

			return f.GetKey(item);
		}

		public static string GetKey(this TypeAccessor t, object item)
		{
			var keys = string.Join("", t.GetMembers().Where(x => x.GetAttribute(typeof(KeyAttribute), false) != null)
				.Select(x => t[item, x.Name].ToString()));

			if (string.IsNullOrEmpty(keys)) throw new InvalidOperationException("Missing Key Attribute");

			return keys;
		}

		public static string GetMemberStringValue<T>(this T item, string memberName)
		{
			var f = TypeAccessor.Create(typeof(T));
			return f[item, memberName] as string;
		}

		public static void AddFields<TSourceType>(this ModelConventionType<TSourceType> modelConventionType)
		{
			var modelConvention = new ModelConvention<TSourceType>();
			modelConventionType.Name = modelConvention.Name;

			if (typeof(TSourceType).GetCustomAttribute(
				typeof(DescriptionAttribute)) is DescriptionAttribute descAttr)
			{
				modelConventionType.Description = descAttr.Description;
			}

			modelConvention.ForEachWithField(
				(type, name, desc, resolve) => modelConventionType.Field(type, name, desc, resolve: resolve), false);
		}

		public static void AddFields<TSourceType>(this ModelConventionInputType<TSourceType> modelConventionInputType)
		{
			var modelConvention = new ModelConvention<TSourceType>();
			modelConventionInputType.Name = $"{modelConvention.Name}Input";
			modelConvention.ForEachWithField(
				(type, name, desc, resolve) => modelConventionInputType.Field(type, name, desc, resolve: resolve), true);
		}

		public static ContainerBuilder UseDataAnnotationsValidation(this ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterType<StringLengthModelValidation>().As<IModelValidation>().SingleInstance();
			containerBuilder.RegisterType<DataAnnotationsValidation>().As<IValidationRule>();
			return containerBuilder;
		}

		/// <summary>
		/// Clones an existing instance.
		/// </summary>
		/// <typeparam name="T">T.</typeparam>
		/// <param name="source">Source instance.</param>
		/// <returns>Clone copy of instance.</returns>
		public static T Clone<T>(this T source)
		{
			var serialized = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<T>(serialized);
		}
	}
}
