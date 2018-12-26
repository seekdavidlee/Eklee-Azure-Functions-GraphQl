using System;
using System.ComponentModel;
using Eklee.Azure.Functions.GraphQl.Attributes;

namespace Eklee.Azure.Functions.GraphQl.Filters
{
	public class DateFilter
	{
		[ModelField(false)]
		[Description("Equal.")]
		public DateTime Equal { get; set; }

		[ModelField(false)]
		[Description("Not equal.")]
		public DateTime NotEqual { get; set; }

		[ModelField(false)]
		[Description("Greater than.")]
		public DateTime GreaterThan { get; set; }

		[ModelField(false)]
		[Description("Greater equal than.")]
		public DateTime GreaterEqualThan { get; set; }

		[ModelField(false)]
		[Description("Less than.")]
		public DateTime LessThan { get; set; }

		[ModelField(false)]
		[Description("Less equal than.")]
		public DateTime LessEqualThan { get; set; }
	}
}
