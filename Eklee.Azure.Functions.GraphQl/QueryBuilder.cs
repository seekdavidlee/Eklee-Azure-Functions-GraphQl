using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Queries;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using FastMember;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Types;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryBuilder<TSource>
	{
		private readonly ObjectGraphType<object> _objectGraphType;
		private readonly string _queryName;
		private readonly string _description;
		private readonly QueryExecutor<TSource> _queryExecutor;
		private readonly IDistributedCache _distributedCache;
		private readonly ILogger _logger;
		private readonly IModelMemberQueryArgumentProvider _modelMemberQueryArgumentProvider;
		private readonly QueryParameterBuilder<TSource> _queryParameterBuilder;
		internal QueryBuilder(ObjectGraphType<object> objectGraphType,
			string queryName,
			string description,
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			IDistributedCache distributedCache,
			ILogger logger,
			IConnectionEdgeHandler connectionEdgeHandler,
			IInMemoryComparerProvider inMemoryComparerProvider,
			IModelMemberQueryArgumentProvider modelMemberQueryArgumentProvider,
			IContextValueResolver contextValueResolver)
		{
			_objectGraphType = objectGraphType;
			_queryName = queryName;
			_description = description;
			_queryExecutor = new QueryExecutor<TSource>(graphQlRepositoryProvider, logger, connectionEdgeHandler);
			_distributedCache = distributedCache;
			_logger = logger;
			_modelMemberQueryArgumentProvider = modelMemberQueryArgumentProvider;
			_queryParameterBuilder = new QueryParameterBuilder<TSource>(this, inMemoryComparerProvider, contextValueResolver);
		}

		private QueryOutput _output;
		private int? _cacheInSeconds;
		private int? _pageLimit;

		public QueryBuilder<TSource> WithCache(TimeSpan timeSpan)
		{
			_cacheInSeconds = Convert.ToInt32(timeSpan.TotalSeconds);
			return this;
		}

		public QueryParameterBuilder<TSource> WithParameterBuilder()
		{
			return _queryParameterBuilder;
		}

		public QueryBuilder<TSource> WithPaging(int pageLimit = 10)
		{
			_pageLimit = pageLimit;
			return this;
		}

		public void BuildWithListResult()
		{
			_output = QueryOutput.List;
			Build();
		}

		public void BuildWithSingleResult()
		{
			_output = QueryOutput.Single;
			Build();
		}

		private Func<ClaimsPrincipal, bool> _claimsPrincipalAssertion;
		public QueryBuilder<TSource> AssertWithClaimsPrincipal(Func<ClaimsPrincipal, bool> claimsPrincipalAssertion)
		{
			_claimsPrincipalAssertion = claimsPrincipalAssertion;
			return this;
		}

		private async Task<object> QueryResolver(ResolveFieldContext<object> context)
		{
			IEnumerable<TSource> list;
			try
			{
				list = await QueryAsync(context);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error occured while performing a query.");
				throw;
			}

			switch (_output)
			{
				case QueryOutput.List:
					return list.ToList();

				case QueryOutput.Single:
					return list.SingleOrDefault();

				default:
					throw new NotImplementedException();
			}
		}

		private async Task<object> ConnectionResolver(ResolveConnectionContext<object> connectionContext)
		{
			IEnumerable<TSource> list = await QueryAsync(connectionContext);

			// ReSharper disable once PossibleInvalidOperationException
			return await connectionContext.GetConnectionAsync(list, _pageLimit.Value);
		}

		private async Task<IEnumerable<TSource>> QueryAsync(ResolveFieldContext<object> context)
		{
			var graphRequestContext = context.UserContext as IGraphRequestContext;

			if (_claimsPrincipalAssertion != null)
			{
				if (graphRequestContext == null || !_claimsPrincipalAssertion(graphRequestContext.HttpRequest.Security.ClaimsPrincipal))
				{
					throw new ExecutionError("Query execution has been denied due to insufficient permissions.", new SecurityException("Query execution has been denied due to insufficient permissions."));
				}
			}

			var steps = _queryParameterBuilder.GetQuerySteps(context).ToList();

			if (steps.Count == 0)
			{
				var qs = _queryParameterBuilder.NewQueryStep();

				var ctxValue = new ContextValue();
				ctxValue.PopulateSelectValues(context);
				Type t = typeof(TSource);
				var ta = TypeAccessor.Create(t);

				qs.QueryParameters = new List<QueryParameter>
				{
						new QueryParameter
						{
							MemberModel = new ModelMember(t, ta, null, true),
							ContextValue = ctxValue
						}
				};

				steps.Add(qs);

			}

			if (_cacheInSeconds > 0)
			{
				var key = steps.GetCacheKey<TSource>();
				_logger.LogInformation($"CacheKey: {key}");

				return (await TryGetOrSetIfNotExistAsync(
					() => _queryExecutor.ExecuteAsync(context.FieldName, steps, graphRequestContext).Result.ToList(), key,
					new DistributedCacheEntryOptions
					{
						// ReSharper disable once PossibleInvalidOperationException
						AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_cacheInSeconds.Value)
					})).Value;
			}

			return await _queryExecutor.ExecuteAsync(context.FieldName, steps, graphRequestContext);
		}

		private void Build()
		{
			switch (_output)
			{
				case QueryOutput.Single:
					_objectGraphType.FieldAsync<ModelConventionType<TSource>>(_queryName,
						description: _description,
						arguments: _modelMemberQueryArgumentProvider.GetQueryArguments(
							_queryParameterBuilder.Members), resolve: QueryResolver);
					break;

				case QueryOutput.List:
					if (_pageLimit > 0)
					{
						var cb = _objectGraphType.Connection<ModelConventionType<TSource>>().Name(_queryName);
						if (!string.IsNullOrEmpty(_description))
						{
							cb.Description(_description);
						}

						_modelMemberQueryArgumentProvider.PopulateConnectionBuilder(cb,
							_queryParameterBuilder.Members);
						cb.ResolveAsync(ConnectionResolver);
					}
					else
					{
						_objectGraphType.FieldAsync<ListGraphType<ModelConventionType<TSource>>>(_queryName,
							description: _description,
							arguments: _modelMemberQueryArgumentProvider.GetQueryArguments(
								_queryParameterBuilder.Members), resolve: QueryResolver);
					}

					break;
			}
		}

		private async Task<ObjectCacheResult<IEnumerable<TSource>>> TryGetOrSetIfNotExistAsync(
			Func<IEnumerable<TSource>> getResult,
			string key,
			DistributedCacheEntryOptions distributedCacheEntryOptions)
		{
			var value = await _distributedCache.GetStringAsync(key);
			if (value != null)
			{
				return new ObjectCacheResult<IEnumerable<TSource>>(JsonConvert.DeserializeObject<List<TSource>>(value), true);
			}

			var result = getResult();

			if (result != null)
				await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(result), distributedCacheEntryOptions);

			return new ObjectCacheResult<IEnumerable<TSource>>(result, false);
		}

		public string QueryName => _queryName;
	}
}