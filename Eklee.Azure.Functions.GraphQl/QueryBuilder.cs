using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

		private readonly List<QueryParameter> _queryParameterList = new List<QueryParameter>();
		private readonly ModelMemberList<TSource> _modelMemberList = new ModelMemberList<TSource>();
		internal QueryBuilder(ObjectGraphType<object> objectGraphType,
			string queryName,
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			IDistributedCache distributedCache)
		{
			_objectGraphType = objectGraphType;
			_queryName = queryName;
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_distributedCache = distributedCache;
		}

		private QueryOutput _output;
		private int? _cacheInSeconds;
		private int? _pageLimit;

		public QueryBuilder<TSource> WithCache(TimeSpan timeSpan)
		{
			_cacheInSeconds = Convert.ToInt32(timeSpan.TotalSeconds);
			return this;
		}

		public QueryBuilder<TSource> WithKeys()
		{
			_modelMemberList.PopulateWithKeyAttribute();

			return this;
		}

		public QueryBuilder<TSource> WithProperty<TProperty>(Expression<Func<TSource, TProperty>> expression, bool isOptional = false)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				_modelMemberList.Add(memberExpression.Member.Name, isOptional);
			}

			return this;
		}

		public QueryBuilder<TSource> WithQueryParameter<TQuery>(Expression<Func<TQuery, object>> expression, bool isOptional = false)
		{
			var modelType = new ModelType<TQuery>();

			if (expression.Body is MemberExpression memberExpression)
			{
				_queryParameterList.Add(new QueryParameter
				{
					Name = memberExpression.Member.Name.ToLower(),
					Description = modelType.GetMember(memberExpression.Member.Name).GetDescription(),
					IsOptional = isOptional
				});
			}

			return this;
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
			var queryParameters = GetQueryParameterList(name => context.Arguments.GetContextValue(name));

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
			var queryParameters = GetQueryParameterList(name => context.Arguments.GetContextValue(name));

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

		private List<QueryParameter> GetQueryParameterList(Func<string, ContextValue> func)
		{
			var list = _modelMemberList.GetQueryParameterList(func).ToList();

			if (_queryParameterList.Count > 0)
			{
				_queryParameterList.ForEach(x => x.ContextValue = func(x.Name));

				list.AddRange(_queryParameterList);
			}
			return list;
		}

		private void Build()
		{
			switch (_output)
			{
				case QueryOutput.Single:
					_objectGraphType.FieldAsync<ModelConventionType<TSource>>(_queryName, arguments: GetQueryArguments(), resolve: QueryResolver);
					break;

				case QueryOutput.List:
					if (_pageLimit > 0)
					{
						var cb = _objectGraphType.Connection<ModelConventionType<TSource>>().Name(_queryName);
						_modelMemberList.ForEach((modelMember, m) =>
						{
							if (m.Type == typeof(string))
								cb = modelMember.IsOptional ?
									cb.Argument<StringGraphType>(modelMember.Name, m.GetDescription()) :
									cb.Argument<NonNullGraphType<StringGraphType>>(modelMember.Name, m.GetDescription());
						});

						_queryParameterList.ForEach(qp =>
						{
							if (qp.IsOptional)
							{
								cb.Argument<StringGraphType>(qp.Name, qp.Description);
							}
							else
							{
								cb.Argument<NonNullGraphType<StringGraphType>>(qp.Name, qp.Description);
							}
						});
						cb.ResolveAsync(ConnectionResolver);

					}
					else
					{
						_objectGraphType.FieldAsync<ListGraphType<ModelConventionType<TSource>>>(_queryName, arguments: GetQueryArguments(), resolve: QueryResolver);
					}

					break;
			}
		}

		private QueryArguments GetQueryArguments()
		{
			var queryArguments = new List<QueryArgument>();

			_queryParameterList.ForEach(x =>
			{
				if (x.IsOptional)
				{
					queryArguments.Add(new QueryArgument<StringGraphType>
					{
						Name = x.Name,
						Description = x.Description
					});
				}
				else
				{
					queryArguments.Add(new QueryArgument<NonNullGraphType<StringGraphType>>
					{
						Name = x.Name,
						Description = x.Description
					});
				}
			});

			_modelMemberList.ForEach((modelMember, m) =>
			{
				if (m.Type == typeof(string))
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<StringGraphType>
						{
							Name = m.Name,
							Description = m.GetDescription()
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<StringGraphType>>
						{
							Name = m.Name,
							Description = m.GetDescription()
						});
					}

					return;
				}

				throw new NotImplementedException();
			});


			return new QueryArguments(queryArguments);
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