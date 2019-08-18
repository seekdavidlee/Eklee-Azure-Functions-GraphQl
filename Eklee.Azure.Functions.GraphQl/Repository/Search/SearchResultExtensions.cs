using FastMember;
using Microsoft.Azure.Search.Models;
using System;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public static class SearchResultExtensions
	{
		public static void AddFacets(this SearchResult searchResult, DocumentSearchResult<Document> results)
		{
			if (results.Facets != null)
			{
				searchResult.Aggregates = results.Facets.Select(f =>

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

		public static void AddValues(this SearchResult searchResult, TypeAccessor typeAccessor, DocumentSearchResult<Document> results)
		{
			var members = typeAccessor.GetMembers();

			results.Results.ToList().ForEach(r =>
			{
				var item = typeAccessor.CreateNew();

				r.Document.ToList().ForEach(d =>
				{
					var field = members.Single(x => x.Name == d.Key);

					object value = d.Value;

					if (field.Type == typeof(Guid) && value is string strValue)
					{
						value = Guid.Parse(strValue);
					}

					if (field.Type == typeof(decimal) && value is double dobValue)
					{
						value = Convert.ToDecimal(dobValue);
					}

					if (field.Type == typeof(DateTime) && value is DateTimeOffset dtmValue)
					{
						value = dtmValue.DateTime;
					}

					if (field.Type == typeof(int))
					{
						value = Convert.ToInt32(value);
					}

					if (field.Type == typeof(long))
					{
						value = Convert.ToInt64(value);
					}

					typeAccessor[item, d.Key] = value;
				});

				searchResult.Values.Add(new SearchResultModel
				{
					Score = r.Score,
					Value = item
				});
			});
		}
	}
}
