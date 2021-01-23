using Azure;
using Azure.Search.Documents.Models;
using FastMember;
using System;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public static class SearchResultExtensions
	{
		public static void AddFacets<T>(this SearchResult searchResult, Response<SearchResults<T>> response)
		{
			if (response.Value.Facets != null)
			{
				searchResult.Aggregates = response.Value.Facets.Select(f =>

					 new SearchAggregateModel
					 {
						 FieldName = f.Key,
						 FieldAggregates = f.Value.Select(v => new FieldAggregateModel { Count = Convert.ToInt32(v.Count), Value = GetFacetStringValue(v) }).ToList()
					 }).ToList();
			}
		}

		private static string GetFacetStringValue(FacetResult result)
		{
			if (result.Value is string strVal)
				return strVal;

			if (result.Value is DateTimeOffset dtoffVal)
				return dtoffVal.ToString("yyyy-MM-dd");

			return result.Value.ToString();
		}

		public static void AddValues<T>(this SearchResult searchResult, TypeAccessor typeAccessor, Response<SearchResults<T>> response)
		{
			var members = typeAccessor.GetMembers();

			response.Value.GetResults().ToList().ForEach(r =>
			{
				searchResult.Values.Add(new SearchResultModel
				{
					Score = r.Score ?? 0,
					Value = r.Document
				});
			});
		}
	}
}
