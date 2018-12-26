using System.ComponentModel;
using Eklee.Azure.Functions.GraphQl.Attributes;

namespace Eklee.Azure.Functions.GraphQl.Filters
{
	public class IntFilter
	{
		[ModelField(false)]
		[Description("Equal.")]
		public int Equal { get; set; }

		[ModelField(false)]
		[Description("Not equal.")]
		public int NotEqual { get; set; }

		[ModelField(false)]
		[Description("Greater than.")]
		public int GreaterThan { get; set; }

		[ModelField(false)]
		[Description("Greater equal than.")]
		public int GreaterEqualThan { get; set; }

		[ModelField(false)]
		[Description("Less than.")]
		public int LessThan { get; set; }

		[ModelField(false)]
		[Description("Less equal than.")]
		public int LessEqualThan { get; set; }
	}
}
