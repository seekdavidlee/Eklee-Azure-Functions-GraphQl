using Eklee.Azure.Functions.GraphQl.Repository;
using GraphQL.Types;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryBuilderFactory
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepository;
		private readonly IDistributedCache _distributedCache;
		private readonly ILogger _logger;

		public QueryBuilderFactory(IGraphQlRepositoryProvider graphQlRepository, 
			IDistributedCache distributedCache,
			ILogger logger)
		{
			_graphQlRepository = graphQlRepository;
			_distributedCache = distributedCache;
			_logger = logger;
		}

		public QueryBuilder<TSource> Create<TSource>(ObjectGraphType<object> objectGraphType, string queryName)
		{
			return new QueryBuilder<TSource>(objectGraphType, queryName, _graphQlRepository, _distributedCache, _logger);
		}
	}
}
