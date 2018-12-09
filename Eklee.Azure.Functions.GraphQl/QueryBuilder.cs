using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using GraphQL.Builders;
using GraphQL.Types;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryBuilder<TSource>
	{
		private readonly ObjectGraphType<object> _objectGraphType;
		private readonly string _queryName;
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly IDistributedCache _distributedCache;

		private readonly QueryParameterBuilder<TSource> _queryParameterBuilder;
		internal QueryBuilder(ObjectGraphType<object> objectGraphType,
			string queryName,
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			IDistributedCache distributedCache)
		{
			_objectGraphType = objectGraphType;
			_queryName = queryName;
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_distributedCache = distributedCache;

			_queryParameterBuilder = new QueryParameterBuilder<TSource>(this);
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

		private async Task<object> QueryResolver(ResolveFieldContext<object> context)
		{
			var queryParameters = _queryParameterBuilder.GetQueryParameterList(name => context.Arguments.GetContextValue(name)).ToList();

			IEnumerable<TSource> list = await QueryAsync(queryParameters);

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

		private async Task<object> ConnectionResolver(ResolveConnectionContext<object> context)
		{
			var queryParameters = _queryParameterBuilder.GetQueryParameterList(name => context.Arguments.GetContextValue(name)).ToList();

			IEnumerable<TSource> list = await QueryAsync(queryParameters);

			// ReSharper disable once PossibleInvalidOperationException
			return await context.GetConnectionAsync(list, _pageLimit.Value);
		}

		private async Task<IEnumerable<TSource>> QueryAsync(List<QueryParameter> queryParameters)
		{
			var key = queryParameters.GetCacheKey();

			IEnumerable<TSource> list;
			if (_cacheInSeconds > 0)
			{
				list = (await TryGetOrSetIfNotExistAsync(
					() => _graphQlRepositoryProvider.QueryAsync<TSource>(queryParameters).Result.ToList(), key,
					new DistributedCacheEntryOptions
					{
						AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_cacheInSeconds.Value)
					})).Value;
			}
			else
			{
				list = await _graphQlRepositoryProvider.QueryAsync<TSource>(queryParameters);
			}

			return list;
		}

		private void Build()
		{
			switch (_output)
			{
				case QueryOutput.Single:
					_objectGraphType.FieldAsync<ModelConventionType<TSource>>(_queryName,
						arguments: _queryParameterBuilder.GetQueryArguments(), resolve: QueryResolver);
					break;

				case QueryOutput.List:
					if (_pageLimit > 0)
					{
						var cb = _objectGraphType.Connection<ModelConventionType<TSource>>().Name(_queryName);
						_queryParameterBuilder.PopulateWithArguments(cb);
						cb.ResolveAsync(ConnectionResolver);
					}
					else
					{
						_objectGraphType.FieldAsync<ListGraphType<ModelConventionType<TSource>>>(_queryName,
							arguments: _queryParameterBuilder.GetQueryArguments(), resolve: QueryResolver);
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
	}
}