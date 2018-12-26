using System.ComponentModel;
using Eklee.Azure.Functions.GraphQl.Attributes;

namespace Eklee.Azure.Functions.GraphQl.Filters
{
	public class GuidFilter
	{
		[ModelField(false)]
		[Description("Equal.")]
		public bool Equal { get; set; }
	}
}
