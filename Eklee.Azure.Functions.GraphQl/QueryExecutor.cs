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

		public async Task<IEnumerable<TSource>> ExecuteAsync(IEnumerable<QueryParameter> queryParameters)
		{
			var list = queryParameters.ToList();
			var hasAnyNestedQueries = list.Count(x => x.MemberModel.IsNested) > 0;

			if (hasAnyNestedQueries)
			{
				throw new NotImplementedException();
			}

			return await _graphQlRepositoryProvider.QueryAsync<TSource>(list);
		}
	}
}
