using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Azure.Functions.Http;
using FastMember;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Http;
using GraphQL.Types;
using GraphQL.Types.Relay;
using GraphQL.Types.Relay.DataObjects;
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
			builder.RegisterType<GraphQlDomain>().As<IGraphQlDomain>().SingleInstance();
			builder.RegisterType<DocumentExecuter>().As<IDocumentExecuter>().SingleInstance();
			builder.RegisterType<DocumentWriter>().As<IDocumentWriter>().SingleInstance();
			builder.RegisterType<TSchema>().As<ISchema>().SingleInstance();

			builder.RegisterType<DocumentDbComparisonInt>().As<IDocumentDbComparison>().SingleInstance();
			builder.RegisterType<DocumentDbComparisonString>().As<IDocumentDbComparison>().SingleInstance();
			builder.RegisterType<DocumentDbComparisonDate>().As<IDocumentDbComparison>().SingleInstance();
			builder.RegisterType<DocumentDbComparisonBool>().As<IDocumentDbComparison>().SingleInstance();
			builder.RegisterType<DocumentDbComparisonGuid>().As<IDocumentDbComparison>().SingleInstance();

			builder.RegisterType<GraphDependencyResolver>().As<IDependencyResolver>();

			builder.RegisterType<QueryBuilderFactory>();
			builder.RegisterType<InputBuilderFactory>();
			builder.RegisterGeneric(typeof(ModelConventionInputType<>));
			builder.RegisterGeneric(typeof(ModelConventionType<>));
			builder.RegisterGeneric(typeof(ConnectionType<>));
			builder.RegisterType<PageInfoType>();
			builder.RegisterGeneric(typeof(EdgeType<>));

			builder.RegisterType<InMemoryRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<HttpRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<DocumentDbRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<SearchRepository>().As<IGraphQlRepository>().SingleInstance();

			builder.RegisterType<GraphQlRepositoryProvider>().As<IGraphQlRepositoryProvider>().SingleInstance();
		}

		public static async Task<IActionResult> ProcessGraphQlRequest(this ExecutionContext executionContext, HttpRequest httpRequest)
		{
			var graphQlDomain = executionContext.Resolve<IGraphQlDomain>();
			var logger = executionContext.Resolve<ILogger>();

			if (httpRequest.ContentType == "application/json")
			{
				var requestBody = await httpRequest.ReadAsStringAsync();

				logger.LogInformation($"Request-Body: {requestBody}");

				return new OkObjectResult(await graphQlDomain.ExecuteAsync(JsonConvert.DeserializeObject<GraphQlDomainRequest>(requestBody)));
			}

			// TODO: Log request Content-Type is unsupported.

			return new BadRequestResult();
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

		public static ContextValue GetContextValue(this Dictionary<string, object> args, ModelMember modelMember)
		{
			var name = modelMember.Name;
			var contextValue = new ContextValue { IsNotSet = !args.ContainsKey(name) };

			if (!contextValue.IsNotSet)
			{
				Dictionary<string, object> arg = (Dictionary<string, object>)args[name];

				if (modelMember.IsGuid)
				{
					contextValue.Value = Guid.Parse(arg.First().Value.ToString());
				}
				else
				{
					contextValue.Value = arg.First().Value;
				}

				if (contextValue.Value == null)
				{
					throw new ArgumentNullException($"{name}.Value");
				}

				string comparison = arg.First().Key;
				if (comparison == "equal")
				{
					contextValue.Comparison = Comparisons.Equal;
					return contextValue;
				}

				if (comparison == "contains" && contextValue.Value is string)
				{
					contextValue.Comparison = Comparisons.StringContains;
					return contextValue;
				}

				if (comparison == "startsWith" && contextValue.Value is string)
				{
					contextValue.Comparison = Comparisons.StringStartsWith;
					return contextValue;
				}

				if (comparison == "endsWith" && contextValue.Value is string)
				{
					contextValue.Comparison = Comparisons.StringEndsWith;
					return contextValue;
				}

				if (comparison == "notEqual" && (
						contextValue.Value is int ||
						contextValue.Value is DateTime))
				{
					contextValue.Comparison = Comparisons.NotEqual;
					return contextValue;
				}

				if (comparison == "greaterThan" && (
						contextValue.Value is int ||
						contextValue.Value is DateTime))
				{
					contextValue.Comparison = Comparisons.GreaterThan;
					return contextValue;
				}

				if (comparison == "greaterEqualThan" && (
						contextValue.Value is int ||
						contextValue.Value is DateTime))
				{
					contextValue.Comparison = Comparisons.GreaterEqualThan;
					return contextValue;
				}

				if (comparison == "lessThan" && (
						contextValue.Value is int ||
						contextValue.Value is DateTime))
				{
					contextValue.Comparison = Comparisons.LessThan;
					return contextValue;
				}

				if (comparison == "lessEqualThan" && (
						contextValue.Value is int ||
						contextValue.Value is DateTime))
				{
					contextValue.Comparison = Comparisons.LessEqualThan;
					return contextValue;
				}
				throw new NotImplementedException($"Comparison: {comparison} is not implemented for type {contextValue.Value.GetType().Name}.");
			}

			return contextValue;
		}

		public static string GetKey<T>(this T item)
		{
			var f = TypeAccessor.Create(typeof(T));

			var keys = string.Join("", f.GetMembers().Where(x => x.GetAttribute(typeof(KeyAttribute), false) != null)
				.Select(x => f[item, x.Name].ToString()));

			if (string.IsNullOrEmpty(keys)) throw new InvalidOperationException("Missing Key Attribute");

			return keys;
		}

		public static void AddFields<TSourceType>(this ModelConventionType<TSourceType> modelConventionType)
		{
			var modelConvention = new ModelConvention<TSourceType>();
			modelConventionType.Name = modelConvention.Name;
			modelConvention.ForEachWithField(
				(type, name, desc) => modelConventionType.Field(type, name, desc));
		}

		public static void AddFields<TSourceType>(this ModelConventionInputType<TSourceType> modelConventionInputType)
		{
			var modelConvention = new ModelConvention<TSourceType>();
			modelConventionInputType.Name = $"{modelConvention.Name}Input";
			modelConvention.ForEachWithField(
				(type, name, desc) => modelConventionInputType.Field(type, name, desc));
		}
	}
}
