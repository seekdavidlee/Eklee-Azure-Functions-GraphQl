using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchResult
	{
		public List<SearchResultModel> Values { get; set; }

		public List<SearchAggregateModel> Aggregates { get; set; }
	}
}
