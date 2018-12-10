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

		public async Task<IEnumerable<TSource>> ExecuteAsync(IEnumerable<QueryParameter> queryParameters, Func<List<List<object>>, List<TSource>> mapper)
		{
			var list = queryParameters.ToList();
			var hasAnyNestedQueries = list.Count(x => x.MemberModel.IsNested) > 0;

			if (hasAnyNestedQueries)
			{
				var dictionary = new Dictionary<string, List<QueryParameter>>();

				list.Where(x => x.MemberModel.IsNested).ToList().ForEach(qp =>
				{
					var typeFullName = qp.MemberModel.SourceType.FullName;
					if (dictionary.ContainsKey(typeFullName))
					{
						dictionary[typeFullName].Add(qp);
					}
					else
					{
						dictionary.Add(typeFullName, new List<QueryParameter> { qp });
					}
				});

				var all = new List<List<object>>();
				foreach (var key in dictionary.Keys)
				{
					all.Add((await _graphQlRepositoryProvider.QueryAsync(dictionary[key])).ToList());
				}

				return mapper(all);
			}

			return await _graphQlRepositoryProvider.QueryAsync<TSource>(list);
		}
	}
}
