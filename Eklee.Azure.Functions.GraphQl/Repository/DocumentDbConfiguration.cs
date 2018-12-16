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
	}

	public class DocumentDbConfiguration<TSource>
	{
		private readonly ModelConventionInputBuilder<TSource> _modelConventionInputBuilder;
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly Type _typeSource;
		private readonly IHttpRequestContext _httpRequestContext;
		private readonly Dictionary<string, object> _configurations = new Dictionary<string, object>();

		public DocumentDbConfiguration(
			ModelConventionInputBuilder<TSource> modelConventionInputBuilder,
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
			_configurations[DocumentDbConstants.Url] = url;
			return this;
		}

		public DocumentDbConfiguration<TSource> AddKey(string key)
		{
			_configurations[DocumentDbConstants.Key] = key;
			return this;
		}

		public DocumentDbConfiguration<TSource> AddDatabase(Func<IHttpRequestContext, string> getDatabase)
		{
			_configurations[DocumentDbConstants.Database] = getDatabase(_httpRequestContext);
			return this;
		}

		public DocumentDbConfiguration<TSource> AddPartition(Expression<Func<TSource, object>> expression)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				_configurations[DocumentDbConstants.Partition] = new PartitionKeyDefinition
				{
					Paths = new Collection<string> { $"/{memberExpression.Member.Name}" }
				};
			}

			return this;
		}

		public DocumentDbConfiguration<TSource> AddRequestUnit(int requestUnit)
		{
			_configurations[DocumentDbConstants.RequestUnit] = requestUnit.ToString();
			return this;
		}

		public ModelConventionInputBuilder<TSource> Build()
		{
			_graphQlRepository.Configure(_typeSource, _configurations);

			return _modelConventionInputBuilder;
		}
	}
}
