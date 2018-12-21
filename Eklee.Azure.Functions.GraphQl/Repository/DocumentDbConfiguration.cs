using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Eklee.Azure.Functions.Http;
using Microsoft.Azure.Documents;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public static class DocumentDbConstants
	{
		public const string Url = "Url";
		public const string Key = "Key";
		public const string Database = "Database";
		public const string RequestUnit = "RequestUnit";
		public const string Partition = "Partition";
		public const string MemberExpression = "MemberExpression";
	}

	public static class DocumentDbConfigurationExtensions
	{
		public static void Add<TSource>(this Dictionary<string, object> configurations, string name, object value)
		{
			configurations[$"{typeof(TSource).Name}{name}"] = value;
		}

		public static T GetValue<T>(this Dictionary<string, object> configurations, string key, Type sourceType)
		{
			return (T)configurations[GetKey(key, sourceType)];
		}


		public static string GetStringValue(this Dictionary<string, object> configurations, string key, Type sourceType)
		{
			return (string)configurations[GetKey(key, sourceType)];
		}

		public static string GetKey(string key, Type sourceType)
		{
			return $"{sourceType.Name}{key}";
		}
	}

	public class DocumentDbConfiguration<TSource>
	{
		private readonly IModelConventionInputBuilder<TSource> _modelConventionInputBuilder;
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly Type _typeSource;
		private readonly IHttpRequestContext _httpRequestContext;
		private readonly Dictionary<string, object> _configurations = new Dictionary<string, object>();

		public DocumentDbConfiguration(
			IModelConventionInputBuilder<TSource> modelConventionInputBuilder,
			IGraphQlRepository graphQlRepository,
			Type typeSource,
			IHttpRequestContext httpRequestContext
			)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
			_graphQlRepository = graphQlRepository;
			_typeSource = typeSource;
			_httpRequestContext = httpRequestContext;
		}

		public DocumentDbConfiguration<TSource> AddUrl(string url)
		{
			_configurations.Add<TSource>(DocumentDbConstants.Url, url);
			return this;
		}


		public DocumentDbConfiguration<TSource> AddKey(string key)
		{
			_configurations.Add<TSource>(DocumentDbConstants.Key, key);
			return this;
		}

		public DocumentDbConfiguration<TSource> AddDatabase(Func<IHttpRequestContext, string> getDatabase)
		{
			_configurations.Add<TSource>(DocumentDbConstants.Database, getDatabase(_httpRequestContext));
			return this;
		}

		public DocumentDbConfiguration<TSource> AddPartition(Expression<Func<TSource, object>> expression)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				_configurations.Add<TSource>(DocumentDbConstants.Partition, new PartitionKeyDefinition
				{
					Paths = new Collection<string> { $"/{memberExpression.Member.Name}" }
				});

				_configurations.Add<TSource>(DocumentDbConstants.MemberExpression, memberExpression);
			}

			return this;
		}

		public DocumentDbConfiguration<TSource> AddRequestUnit(int requestUnit)
		{
			_configurations.Add<TSource>(DocumentDbConstants.RequestUnit, requestUnit.ToString());
			return this;
		}

		public IModelConventionInputBuilder<TSource> BuildDocumentDb()
		{
			if (!_configurations.ContainsKey(DocumentDbConstants.Partition))
			{
				throw new InvalidOperationException("Partition is not set!");
			}

			_graphQlRepository.Configure(_typeSource, _configurations);

			return _modelConventionInputBuilder;
		}
	}
}
