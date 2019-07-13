using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryExecutor<TSource>
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ILogger _logger;
		private readonly IConnectionEdgeHandler _connectionEdgeHandler;

		public QueryExecutor(IGraphQlRepositoryProvider graphQlRepositoryProvider, ILogger logger, IConnectionEdgeHandler connectionEdgeHandler)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_logger = logger;
			_connectionEdgeHandler = connectionEdgeHandler;
		}

		public async Task<IEnumerable<TSource>> ExecuteAsync(string queryName, IEnumerable<QueryStep> querySteps, IGraphRequestContext graphRequestContext)
		{
			var ctx = new QueryExecutionContext();

			foreach (var queryStep in querySteps)
			{
				queryStep.Started = DateTime.UtcNow;

				if (queryStep.Mapper != null)
				{
					// We may have to make several queries.
					var nextQueryResults = new List<object>();

					var queryValues = queryStep.Mapper(ctx);

					if (queryValues.Count > 0)
					{
						var first = queryStep.QueryParameters.First();

						first.ContextValue = new ContextValue { Values = queryValues, Comparison = Comparisons.Equal };

						try
						{
							var results = await QueryAsync(queryName, queryStep, graphRequestContext);
							nextQueryResults.AddRange(results);
						}
						catch (Exception e)
						{
							_logger.LogError(e, "An error has occured while executing query on repository.");
							throw;
						}
					}
					else
					{
						_logger.LogWarning($"No values detected for queryStep @ {queryName}");
					}

					ctx.SetQueryResult(nextQueryResults);
				}
				else
				{
					ctx.SetQueryResult(await QueryAsync(queryName, queryStep, graphRequestContext));
				}

				queryStep.ContextAction?.Invoke(ctx);

				queryStep.Ended = DateTime.UtcNow;
			}

			return ctx.GetResults<TSource>();
		}

		private async Task<List<object>> QueryAsync(string queryName, QueryStep queryStep, IGraphRequestContext graphRequestContext)
		{
			var results = (await _graphQlRepositoryProvider.QueryAsync(queryName, queryStep, graphRequestContext)).ToList();

			if (results.Count > 0)
			{
				if (results.First().GetType() == typeof(SearchResultModel))
				{
					return results;
				}

				await _connectionEdgeHandler.QueryAsync(results, queryStep, graphRequestContext);
			}
			return results;
		}
	}
}
