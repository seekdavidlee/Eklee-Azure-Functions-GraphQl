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
		[Description("Smaller than.")]
		public int SmallerThan { get; set; }

		[ModelField(false)]
		[Description("Smaller equal than.")]
		public int SmallerEqualThan { get; set; }
	}
}
