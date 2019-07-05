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
		private readonly IQueryArgumentsBuilder _queryArgumentsBuilder;
		private readonly IFieldMutationResolver _fieldMutationResolver;

		public InputBuilderFactory(
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			ILogger logger,
			ISearchMappedModels searchMappedModels,
			IQueryArgumentsBuilder queryArgumentsBuilder,
			IFieldMutationResolver fieldMutationResolver)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_logger = logger;
			_searchMappedModels = searchMappedModels;
			_queryArgumentsBuilder = queryArgumentsBuilder;
			_fieldMutationResolver = fieldMutationResolver;
		}

		public ModelConventionInputBuilder<TSource> Create<TSource>(ObjectGraphType objectGraphType) where TSource : class
		{
			try
			{
				_logger.LogInformation($"Creating model meta data for {typeof(TSource)}.");

				return new ModelConventionInputBuilder<TSource>(
					objectGraphType,
					_graphQlRepositoryProvider,
					_searchMappedModels,
					_queryArgumentsBuilder,
					_fieldMutationResolver);
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"An error has occured while creating model meta data for {typeof(TSource)}.");
				throw;
			}
		}
	}
}
