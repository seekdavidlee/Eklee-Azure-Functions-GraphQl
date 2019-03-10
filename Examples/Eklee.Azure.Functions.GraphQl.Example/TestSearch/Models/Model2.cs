using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch.Models
{
	[Description("Types of Model1s")]
	public enum Model2Types
	{
		[Description("Model 2 Type 1")]
		M2Type1,

		[Description("Model 2 Type 2")]
		M2Type2
	}

	public class Model2
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

		[Description("Model2Types")]
		public Model2Types? Type { get; set; }
	}
}