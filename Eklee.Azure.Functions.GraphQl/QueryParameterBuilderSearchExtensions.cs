using Eklee.Azure.Functions.GraphQl.Repository.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	/// <summary>
	/// Extension class for building search queries.
	/// </summary>
	public static class QueryParameterBuilderSearchExtensions
	{
		/// <summary>
		/// Begin Search Builder. 
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="qb">QueryBuilder passed in.</param>
		/// <param name="searchTypes">Array of types you would like to perform searches on.</param>
		/// <returns>QueryStepBuilder.</returns>
		[Obsolete("Method is deprecated in the next version, please use method which does not take in any types instead.")]
		public static QueryStepBuilder<TSource, SearchModel> BeginSearch<TSource>(
			this QueryParameterBuilder<TSource> qb,
			params Type[] searchTypes)
		{
			var step = Create(qb);
			step.AddStepBagItem(SearchConstants.QueryTypes, searchTypes);
			return step;
		}

		/// <summary>
		/// Begin Search Builder. 
		/// </summary>
		/// <typeparam name="TSource">Search source type.</typeparam>
		/// <param name="qb">QueryBuilder instance.</param>
		/// <returns>SearchQueryBuilder.</returns>
		public static SearchQueryBuilder<TSource> BeginSearch<TSource>(
			this QueryParameterBuilder<TSource> qb)
		{
			return new SearchQueryBuilder<TSource>(Create(qb));
		}

		/// <summary>
		/// Creates a new instance of SearchQueryBuilder.
		/// </summary>
		/// <typeparam name="TSource">Search source type.</typeparam>
		/// <param name="qb">QueryBuilder instance.</param>
		/// <returns>QueryStepBuilder.</returns>
		private static QueryStepBuilder<TSource, SearchModel> Create<TSource>(QueryParameterBuilder<TSource> qb)
		{
			var step = new QueryStepBuilder<TSource, SearchModel>(qb);
			step.DisableConnectionEdgeCheck();
			step.OverrideRepositoryTypeWith<SearchResult>();
			step.AddStepBagItem(SearchConstants.QueryName, qb.GetQueryBuilderQueryName());
			step.WithProperty(x => x.SearchText);
			step.AddQueryExecutionContextInterceptor(ctx =>
			{
				// This step is for backwards compatibility.
				var searchResults = ctx.GetQueryResults<SearchResult>();

				var list = new List<SearchResultModel>();

				searchResults.ForEach(sr => list.AddRange(sr.Values));

				ctx.SystemItems[typeof(SearchResult).FullName] = searchResults;

				ctx.SetQueryResult(list.Select(x => (object)x).ToList());

			});
			return step;
		}
	}
}
