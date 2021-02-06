using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model19
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[Description("Double")]
		public decimal Value { get; set; }

		[Description("Double 2")]
		public decimal? Value2 { get; set; }
	}
}
