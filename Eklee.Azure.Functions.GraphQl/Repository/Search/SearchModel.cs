using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchModel
	{
		[Description("Search text.")]
		public string SearchText { get; set; }
	}
}
