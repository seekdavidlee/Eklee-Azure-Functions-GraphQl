using System.ComponentModel;
using Eklee.Azure.Functions.GraphQl.Attributes;

namespace Eklee.Azure.Functions.GraphQl.Filters
{
	public class GuidFilter
	{
		[ModelField(false)]
		[Description("Equal.")]
		public string Equal { get; set; }
	}
}
