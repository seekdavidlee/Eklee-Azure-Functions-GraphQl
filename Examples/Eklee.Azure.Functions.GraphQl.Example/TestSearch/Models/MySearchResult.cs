using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch.Models
{
	public class MySearchResult
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("IntField")]
		public int IntField { get; set; }

		[Description("DoubleField")]
		public double DoubleField { get; set; }


		[Description("DateField")]
		public DateTime DateField { get; set; }


		[Description("Field")]
		public string Field { get; set; }
	}
}
