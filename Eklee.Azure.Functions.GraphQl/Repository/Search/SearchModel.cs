using System.Collections.Generic;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchModel
	{
		[Description("Search text.")]
		public string SearchText { get; set; }

		[Description("List of filters.")]
		public List<SearchFilterModel> Filters { get; set; }
	}
}
