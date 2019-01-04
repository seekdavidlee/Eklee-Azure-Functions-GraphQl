using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchConfiguration<TSource> where TSource : class
	{
		private readonly IModelConventionInputBuilder<TSource> _modelConventionInputBuilder;
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly Type _typeSource;
		private readonly Dictionary<string, object> _configurations = new Dictionary<string, object>();

		public SearchConfiguration(
			IModelConventionInputBuilder<TSource> modelConventionInputBuilder,
			IGraphQlRepository graphQlRepository,
			Type typeSource)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
			_graphQlRepository = graphQlRepository;
			_typeSource = typeSource;
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

		public IModelConventionInputBuilder<TSource> BuildSearch()
		{
			_graphQlRepository.Configure(_typeSource, _configurations);
			
			return _modelConventionInputBuilder;
		}
	}
}