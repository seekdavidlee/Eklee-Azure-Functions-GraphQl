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

		/// <summary>
		/// Constructor for SearchQueryBuilder.
		/// </summary>
		/// <param name="queryStepBuilder">QueryStepBuilder.</param>
		public SearchQueryBuilder(QueryStepBuilder<TSource, SearchModel> queryStepBuilder)
		{
			_queryStepBuilder = queryStepBuilder;
		}

		/// <summary>
		/// Adds Search model.
		/// </summary>
		/// <typeparam name="TSearchModel">Search model.</typeparam>
		/// <returns>SearchQueryBuilder.</returns>
		public SearchQueryBuilder<TSource> Add<TSearchModel>()
		{
			_searchTypes.Add(typeof(TSearchModel));
			return this;
		}

		/// <summary>
		/// Builds the search query with search aggregations enabled.  
		/// </summary>
		/// <remarks>Indicates that we will return search aggregations when we perform searches.</remarks>
		/// <returns>QueryStepBuilder.</returns>
		public QueryStepBuilder<TSource, SearchModel> BuildWithAggregate()
		{
			// We just need this key to indicate we are enabling aggregating, and do not need a value.
			_queryStepBuilder.AddStepBagItem(SearchConstants.EnableAggregate, null);
			_queryStepBuilder.WithProperty(x => x.Filters);
			return Build();
		}

		/// <summary>
		/// Builds the search query.
		/// </summary>
		/// <returns>QueryStepBuilder.</returns>
		public QueryStepBuilder<TSource, SearchModel> Build()
		{
			_queryStepBuilder.AddStepBagItem(SearchConstants.QueryTypes, _searchTypes.ToArray());
			return _queryStepBuilder;
		}
	}
}
