using Eklee.Azure.Functions.GraphQl.Repository.Search;
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
