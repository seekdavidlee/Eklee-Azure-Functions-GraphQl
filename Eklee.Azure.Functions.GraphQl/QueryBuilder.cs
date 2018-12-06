using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
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
		private readonly IGraphQlRepositoryProvider _graphQlRepository;
		private readonly IDistributedCache _distributedCache;
		private readonly List<ModelMember> _modelMemberList = new List<ModelMember>();
		private readonly TypeAccessor _typeAccessor;
		private readonly List<Member> _members;

		internal QueryBuilder(ObjectGraphType<object> objectGraphType,
			string queryName,
			IGraphQlRepositoryProvider graphQlRepository,
			IDistributedCache distributedCache)
		{
			_objectGraphType = objectGraphType;
			_queryName = queryName;
			_graphQlRepository = graphQlRepository;
			_distributedCache = distributedCache;

			_typeAccessor = TypeAccessor.Create(typeof(TSource));
			_members = _typeAccessor.GetMembers().ToList();
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
			_members.ForEach(m =>
			{
				if ((KeyAttribute)m.GetAttribute(typeof(KeyAttribute), false) != null)
				{
					_modelMemberList.Add(m.Name.ToLower(), false);
				}
			});
			return this;
		}

		public QueryBuilder<TSource> WithProperty<TProperty>(Expression<Func<TSource, TProperty>> expression, bool isOptional = false)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				_modelMemberList.Add(memberExpression.Member.Name.ToLower(), isOptional);
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

		async Task<object> QueryResolver(ResolveFieldContext<object> context)
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

		async Task<object> ConnectionResolver(ResolveConnectionContext<object> context)
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
					() => _graphQlRepository.QueryAsync<TSource>(queryParameters).Result.ToList(), key,
					new DistributedCacheEntryOptions
					{
						AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_cacheInSeconds.Value)
					})).Value;
			}
			else
			{
				list = await _graphQlRepository.QueryAsync<TSource>(queryParameters);
			}

			return list;
		}

		private List<QueryParameter> GetQueryParameterList(Func<string, ContextValue> func)
		{
			return _modelMemberList.Select(memberSetup =>
			{
				var queryParameter = new QueryParameter
				{
					ContextValue = func(memberSetup.Name),
					Member = _members.Single(x => x.Name.ToLower() == memberSetup.Name),
					MemberParent = _typeAccessor,
					IsOptional = memberSetup.IsOptional
				};
				return queryParameter;
			}).ToList();
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
						_modelMemberList.ForEach(memberSetup =>
						{
							var m = _members.GetMember(memberSetup.Name);

							if (m.Type == typeof(string))
								cb = memberSetup.IsOptional ?
									cb.Argument<StringGraphType>(memberSetup.Name, m.GetDescription()) :
									cb.Argument<NonNullGraphType<StringGraphType>>(memberSetup.Name, m.GetDescription());
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

			_modelMemberList.ForEach(memberSetup =>
			{
				var m = _members.GetMember(memberSetup.Name);

				if (m.Type == typeof(string))
				{
					if (memberSetup.IsOptional)
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