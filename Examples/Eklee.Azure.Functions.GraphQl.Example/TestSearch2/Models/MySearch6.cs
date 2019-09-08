using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2.Models
{
	public class MySearch6
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("StrField")]
		public string StrField { get; set; }

		public List<MySearch7> List7 { get; set; }

		public MySearch8 MySearch8 { get; set; } 
	}
}