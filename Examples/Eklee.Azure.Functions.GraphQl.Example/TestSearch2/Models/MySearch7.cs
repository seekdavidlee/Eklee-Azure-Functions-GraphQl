using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2.Models
{
	public class MySearch7
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("DobField")]
		public double DobField { get; set; }
	}
}