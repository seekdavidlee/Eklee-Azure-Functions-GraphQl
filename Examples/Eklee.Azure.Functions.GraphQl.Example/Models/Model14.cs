using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model14
	{
		[Key]
		[Description("Some Key")]
		public string SomeKey { get; set; }

		[Description("Some Descr")]
		public string Descr { get; set; }
	}
}
