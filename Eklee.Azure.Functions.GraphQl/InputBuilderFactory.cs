using System;
using Eklee.Azure.Functions.GraphQl.Repository;
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

		public InputBuilderFactory(
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			ILogger logger,
			IHttpRequestContext httpRequestContext)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_logger = logger;
			_httpRequestContext = httpRequestContext;
		}

		public ModelConventionInputBuilder<TSource> Create<TSource>(ObjectGraphType objectGraphType)
		{
			try
			{
				_logger.LogInformation($"Creating model meta data for {typeof(TSource)}.");

				return new ModelConventionInputBuilder<TSource>(
					objectGraphType,
					_graphQlRepositoryProvider,
					_logger,
					_httpRequestContext);
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"An error has occured while creating model meta data for {typeof(TSource)}.");
				throw;
			}
		}
	}
}
