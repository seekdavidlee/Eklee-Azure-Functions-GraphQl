using System;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Azure.Functions.Http;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class InputBuilderFactory
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ILogger _logger;
		private readonly IHttpRequestContext _httpRequestContext;
		private readonly ISearchMappedModels _searchMappedModels;

		public InputBuilderFactory(
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			ILogger logger,
			IHttpRequestContext httpRequestContext,
			ISearchMappedModels searchMappedModels)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_logger = logger;
			_httpRequestContext = httpRequestContext;
			_searchMappedModels = searchMappedModels;
		}

		public ModelConventionInputBuilder<TSource> Create<TSource>(ObjectGraphType objectGraphType) where TSource : class
		{
			try
			{
				_logger.LogInformation($"Creating model meta data for {typeof(TSource)}.");

				return new ModelConventionInputBuilder<TSource>(
					objectGraphType,
					_graphQlRepositoryProvider,
					_logger,
					_httpRequestContext,
					_searchMappedModels);
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"An error has occured while creating model meta data for {typeof(TSource)}.");
				throw;
			}
		}
	}
}
