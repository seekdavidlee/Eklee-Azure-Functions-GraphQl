using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Queries;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
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
		private readonly IConnectionEdgeHandler _connectionEdgeHandler;
		private readonly IInMemoryComparerProvider _inMemoryComparerProvider;
		private readonly IModelMemberQueryArgumentProvider _modelMemberQueryArgumentProvider;
		private readonly IContextValueResolver _contextValueResolver;

		public QueryBuilderFactory(IGraphQlRepositoryProvider graphQlRepository,
			IDistributedCache distributedCache,
			ILogger logger,
			IConnectionEdgeHandler connectionEdgeHandler,
			IInMemoryComparerProvider inMemoryComparerProvider,
			IModelMemberQueryArgumentProvider modelMemberQueryArgumentProvider,
			IContextValueResolver contextValueResolver)
		{
			_graphQlRepository = graphQlRepository;
			_distributedCache = distributedCache;
			_logger = logger;
			_connectionEdgeHandler = connectionEdgeHandler;
			_inMemoryComparerProvider = inMemoryComparerProvider;
			_modelMemberQueryArgumentProvider = modelMemberQueryArgumentProvider;
			_contextValueResolver = contextValueResolver;
		}

		public QueryBuilder<TSource> Create<TSource>(ObjectGraphType<object> objectGraphType, string queryName, string description = null)
		{
			return new QueryBuilder<TSource>(objectGraphType, queryName, description,
				_graphQlRepository,
				_distributedCache,
				_logger,
				_connectionEdgeHandler,
				_inMemoryComparerProvider,
				_modelMemberQueryArgumentProvider,
				_contextValueResolver);
		}
	}
}
