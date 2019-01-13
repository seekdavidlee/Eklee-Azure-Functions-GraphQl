using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Eklee.Azure.Functions.Http;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbConfiguration<TSource> where TSource : class
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

		public DocumentDbConfiguration<TSource> AddPartition<TProperty>(Expression<Func<TSource, TProperty>> expression)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				_configurations.Add<TSource>(DocumentDbConstants.PartitionMemberExpression, memberExpression);
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
			if (!_configurations.ContainsKey<TSource>(DocumentDbConstants.PartitionMemberExpression))
			{
				throw new InvalidOperationException("Partition is not set!");
			}

			_graphQlRepository.Configure(_typeSource, _configurations);

			return _modelConventionInputBuilder;
		}
	}
}
