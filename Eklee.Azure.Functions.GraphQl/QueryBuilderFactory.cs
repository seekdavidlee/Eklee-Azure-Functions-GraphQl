using Eklee.Azure.Functions.GraphQl.Connections;
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
		private readonly IConnectionEdgeResolver _connectionEdgeResolver;

		public QueryBuilderFactory(IGraphQlRepositoryProvider graphQlRepository,
			IDistributedCache distributedCache,
			ILogger logger,
			IConnectionEdgeResolver connectionEdgeResolver)
		{
			_graphQlRepository = graphQlRepository;
			_distributedCache = distributedCache;
			_logger = logger;
			_connectionEdgeResolver = connectionEdgeResolver;
		}

		public QueryBuilder<TSource> Create<TSource>(ObjectGraphType<object> objectGraphType, string queryName, string description = null)
		{
			return new QueryBuilder<TSource>(objectGraphType, queryName, description, _graphQlRepository, _distributedCache, _logger, _connectionEdgeResolver);
		}
	}
}
