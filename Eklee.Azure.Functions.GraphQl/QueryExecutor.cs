using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryExecutor<TSource>
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ILogger _logger;

		public QueryExecutor(IGraphQlRepositoryProvider graphQlRepositoryProvider, ILogger logger)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_logger = logger;
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
							var results = (await _graphQlRepositoryProvider.QueryAsync(queryName, queryStep, graphRequestContext)).ToList();
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
					ctx.SetQueryResult((await _graphQlRepositoryProvider.QueryAsync(queryName, queryStep, graphRequestContext)).ToList());
				}

				queryStep.ContextAction?.Invoke(ctx);

				queryStep.Ended = DateTime.UtcNow;
			}

			return ctx.GetResults<TSource>();
		}
	}
}
