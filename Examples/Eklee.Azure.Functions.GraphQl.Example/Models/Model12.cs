using Eklee.Azure.Functions.GraphQl.Attributes;
using Eklee.Azure.Functions.GraphQl.Example.Actions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model12
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[ModelField(false)]
		[Description("Field")]
		public string Field { get; set; }

		[RequestContextValue(typeof(ValueFromRequestHeader))]
		[ModelField(false)]
		[Description("FieldFromHeader")]
		public string FieldFromHeader { get; set; }

	}
}
