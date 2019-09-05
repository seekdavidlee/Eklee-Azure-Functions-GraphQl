using Eklee.Azure.Functions.GraphQl.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model12
	{
		[AutoId(AutoIdPatterns.Guid)]
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[ModelField(false)]
		[Description("Field")]
		public string Field { get; set; }
	}
}
