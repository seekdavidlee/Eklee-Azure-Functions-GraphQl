using System.Collections.Generic;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchAggregateModel
	{
		[Description("Name of the field.")]
		public string FieldName { get; set; }

		[Description("List of aggregates.")]
		public List<FieldAggregateModel> FieldAggregates { get; set; }
	}
}
