using System.ComponentModel;
using Eklee.Azure.Functions.GraphQl.Attributes;

namespace Eklee.Azure.Functions.GraphQl.Filters
{
	public class StringEqualFilter
	{
		[ModelField(false)]
		[Description("String equal.")]
		public string Equal { get; set; }
	}
}
