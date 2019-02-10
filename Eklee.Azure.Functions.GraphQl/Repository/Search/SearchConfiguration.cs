using System;
using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchConfiguration<TSource> where TSource : class
	{
		private readonly IModelConventionInputBuilder<TSource> _modelConventionInputBuilder;
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly Dictionary<string, object> _configurations = new Dictionary<string, object>();

		public SearchConfiguration(
			IModelConventionInputBuilder<TSource> modelConventionInputBuilder,
			IGraphQlRepository graphQlRepository)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
			_graphQlRepository = graphQlRepository;
		}

		public SearchConfiguration<TSource> AddGraphRequestContextSelector(Func<IGraphRequestContext, bool> selector)
		{
			_configurations.Add<TSource>(SearchConstants.RequestContextSelector, selector);
			return this;
		}

		public SearchConfiguration<TSource> AddServiceName(string serviceName)
		{
			_configurations.Add<TSource>(SearchConstants.ServiceName, serviceName);
			return this;
		}

		public SearchConfiguration<TSource> AddApiKey(string apiKey)
		{
			_configurations.Add<TSource>(SearchConstants.ApiKey, apiKey);
			return this;
		}

		public SearchConfiguration<TSource> AddPrefix(string prefix)
		{
			_configurations.Add<TSource>(SearchConstants.Prefix, prefix);
			return this;
		}

		public IModelConventionInputBuilder<TSource> BuildSearch()
		{
			_graphQlRepository.Configure(typeof(TSource), _configurations);
			
			return _modelConventionInputBuilder;
		}
	}
}