using System.ComponentModel;
using Eklee.Azure.Functions.GraphQl.Attributes;

namespace Eklee.Azure.Functions.GraphQl.Filters
{
	public class FieldNameValueFilter
	{
		[ModelField(false)]
		[Description("Field name.")]
		public string FieldName { get; set; }

		[ModelField(false)]
		[Description("Field value.")]
		public string FieldValue { get; set; }

		[ModelField(false)]
		[Description("Comparisons.")]
		public Comparisons Comparison { get; set; }
	}
}
