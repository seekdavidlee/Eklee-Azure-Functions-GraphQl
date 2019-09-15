using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository;
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
			var ctx = new QueryExecutionContext(graphRequestContext);

			foreach (var queryStep in querySteps)
			{
				queryStep.Started = DateTime.UtcNow;

				// If there are any runtime mappers, run them first.
				queryStep.QueryParameters.ForEach(qp =>
				{
					if (qp.Mapper != null)
					{
						var values = qp.Mapper(new MapperQueryExecutionContext(ctx, queryStep));
						if (values.Count > 0)
						{
							qp.ContextValue = new ContextValue
							{
								Values = values,
								Comparison = Comparisons.Equal
							};
						}
					}
				});

				var nextQueryResults = new List<object>();

				if (queryStep.StepMapper != null)
				{
					var queryValues = queryStep.StepMapper(new MapperQueryExecutionContext(ctx, queryStep));

					if (queryValues.Count > 0)
					{
						var first = queryStep.QueryParameters.FirstOrDefault(x => x.Rule != null && x.Rule.PopulateWithQueryValues);
						if (first == null)
						{
							first = queryStep.QueryParameters.First();
						}

						first.ContextValue = new ContextValue
						{
							Values = queryValues,
							Comparison = Comparisons.Equal,
							SelectValues = first.ContextValue != null && first.ContextValue.SelectValues != null ? first.ContextValue.SelectValues : null
						};

						try
						{
							nextQueryResults.AddRange(await QueryAsync(queryName, queryStep, graphRequestContext));
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
				}
				else
				{
					if (!IsInvalidForNextQuery(queryStep))
					{
						nextQueryResults.AddRange(await QueryAsync(queryName, queryStep, graphRequestContext));
					}
					else
					{
						_logger.LogWarning($"Invalid values detected for queryStep @ {queryName}");
					}
				}

				ctx.SetQueryResult(nextQueryResults);

				queryStep.ContextAction?.Invoke(ctx);

				queryStep.Ended = DateTime.UtcNow;
			}

			return ctx.GetResults<TSource>();
		}

		private static bool IsInvalidForNextQuery(QueryStep queryStep)
		{
			if (queryStep.QueryParameters.Count == 1 &&
				queryStep.QueryParameters.Single().Mapper != null)
			{
				var qp = queryStep.QueryParameters.Single();

				return qp.ContextValue == null || (qp.ContextValue.Values == null);
			}

			return false;
		}

		private async Task<List<object>> QueryAsync(string queryName, QueryStep queryStep, IGraphRequestContext graphRequestContext)
		{
			var results = (await _graphQlRepositoryProvider.QueryAsync(queryName, queryStep, graphRequestContext)).ToList();

			if (!queryStep.SkipConnectionEdgeCheck && results.Count > 0)
			{
				await _connectionEdgeHandler.QueryAsync(results, queryStep, graphRequestContext);
			}
			return results;
		}
	}
}
