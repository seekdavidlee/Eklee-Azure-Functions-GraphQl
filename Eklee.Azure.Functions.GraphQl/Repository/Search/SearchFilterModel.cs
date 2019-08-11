using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchFilterModel
	{
		[Description("Name of the field.")]
		public string FieldName { get; set; }

		[Description("Field filter value.")]
		public string Value { get; set; }

		[Description("Field filter comprison.")]
		public Comparisons Comprison { get; set; }
	}
}
