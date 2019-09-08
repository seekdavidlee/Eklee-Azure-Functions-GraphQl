using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2.Models
{
	public class MySearch8
	{
		[Description("Id")]
		public string Id { get; set; }

		[Description("StrField")]
		public string StrField { get; set; }
	}
}