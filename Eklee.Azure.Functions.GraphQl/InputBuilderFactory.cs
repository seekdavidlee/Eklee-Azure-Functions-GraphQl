using System;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class InputBuilderFactory
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ILogger _logger;
		private readonly ISearchMappedModels _searchMappedModels;

		public InputBuilderFactory(
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			ILogger logger,
			ISearchMappedModels searchMappedModels)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_logger = logger;
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
