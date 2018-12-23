using System.ComponentModel;
using Eklee.Azure.Functions.GraphQl.Attributes;

namespace Eklee.Azure.Functions.GraphQl.Filters
{
	public class StringFilter
	{
		[ModelField(false)]
		[Description("String equal.")]
		public string Equal { get; set; }

		[ModelField(false)]
		[Description("String starts with.")]
		public string StartsWith { get; set; }

		[ModelField(false)]
		[Description("String ends with.")]
		public string EndsWith { get; set; }

		[ModelField(false)]
		[Description("String contains.")]
		public string Contains { get; set; }
	}
}
