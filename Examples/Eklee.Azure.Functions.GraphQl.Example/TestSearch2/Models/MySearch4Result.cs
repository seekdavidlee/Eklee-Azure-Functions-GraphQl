using Eklee.Azure.Functions.GraphQl.Repository.Search;
using System.Collections.Generic;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2.Models
{
	public class MySearch4Result
	{
		[Description("Results")]
		public List<MySearch4> Results { get; set; }

		[Description("List of search aggregates.")]
		public List<SearchAggregateModel> Aggregates { get; set; }
	}
}
