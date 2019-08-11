using Eklee.Azure.Functions.GraphQl.Repository.Search;
using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl
{
	/// <summary>
	/// Supports fluent way of ochestrating the search types.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	public class SearchQueryBuilder<TSource>
	{
		private readonly List<Type> _searchTypes = new List<Type>();
		private readonly QueryStepBuilder<TSource, SearchModel> _queryStepBuilder;

		public SearchQueryBuilder(QueryStepBuilder<TSource, SearchModel> queryStepBuilder)
		{
			_queryStepBuilder = queryStepBuilder;
		}

		public SearchQueryBuilder<TSource> Add<TSearchModel>()
		{
			_searchTypes.Add(typeof(TSearchModel));
			return this;
		}

		public QueryStepBuilder<TSource, SearchModel> BuildWithAggregate()
		{
			_queryStepBuilder.AddStepBagItem(SearchConstants.EnableAggregate, "");
			return Build();
		}

		public QueryStepBuilder<TSource, SearchModel> Build()
		{
			_queryStepBuilder.AddStepBagItem(SearchConstants.QueryTypes, _searchTypes.ToArray());
			return _queryStepBuilder;
		}
	}
}
