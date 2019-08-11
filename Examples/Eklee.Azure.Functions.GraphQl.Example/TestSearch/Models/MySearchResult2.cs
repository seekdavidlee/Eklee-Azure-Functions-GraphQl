using Eklee.Azure.Functions.GraphQl.Repository.Search;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch.Models
{
	public class MySearchResult2
	{
		public MySearchResult2()
		{
			Aggregates = new List<SearchAggregateModel>();
		}

		public List<MySearchResult> Results { get; set; }

		public List<SearchAggregateModel> Aggregates { get; set; }
	}
}
