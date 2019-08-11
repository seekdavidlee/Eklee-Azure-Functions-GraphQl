using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class FieldAggregateModel
	{
		[Description("Aggregate value.")]
		public string Value { get; set; }

		[Description("Aggregate count.")]
		public int Count { get; set; }
	}
}
