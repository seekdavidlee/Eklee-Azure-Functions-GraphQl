using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model17
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[Description("Number")]
		public int Value { get; set; }

		[Description("Number 2")]
		public int? Value2 { get; set; }
	}
}
