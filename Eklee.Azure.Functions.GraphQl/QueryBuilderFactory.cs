using GraphQL.Types;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryBuilderFactory
	{
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly IDistributedCache _distributedCache;

		public QueryBuilderFactory(IGraphQlRepository graphQlRepository, IDistributedCache distributedCache)
		{
			_graphQlRepository = graphQlRepository;
			_distributedCache = distributedCache;
		}

		public QueryBuilder<TSource> Create<TSource>(ObjectGraphType<object> objectGraphType, string queryName)
		{
			return new QueryBuilder<TSource>(objectGraphType, queryName, _graphQlRepository, _distributedCache);
		}
	}
}
