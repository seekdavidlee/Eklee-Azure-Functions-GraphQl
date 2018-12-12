using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryExecutor<TSource>
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;

		public QueryExecutor(IGraphQlRepositoryProvider graphQlRepositoryProvider)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
		}

		public async Task<IEnumerable<TSource>> ExecuteAsync(IEnumerable<QueryStep> querySteps)
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

					var first = queryStep.QueryParameters.First();

					foreach (var queryValue in queryValues)
					{
						first.ContextValue = new ContextValue { Value = queryValue };
						var results = (await _graphQlRepositoryProvider.QueryAsync(queryStep.QueryParameters)).ToList();
						nextQueryResults.AddRange(results);
					}

					ctx.SetQueryResult(nextQueryResults);
				}
				else
				{
					ctx.SetQueryResult((await _graphQlRepositoryProvider.QueryAsync(queryStep.QueryParameters)).ToList());
				}

				queryStep.ContextAction?.Invoke(ctx);

				queryStep.Ended = DateTime.UtcNow;
			}

			return ctx.GetResults<TSource>();
		}
	}
}
