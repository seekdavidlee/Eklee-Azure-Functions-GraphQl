using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Eklee.Azure.Functions.GraphQl.Attributes;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	[Description("Types of Model1s")]
	public enum Model1Types
	{
		[Description("Model 1 Type 1")]
		M1Type1,

		[Description("Model 1 Type 2")]
		M1Type2
	}

	[Description("Model 1 class for testing Search.")]
	public class Model1
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

		[ModelField(true)]
		[Description("Types")]
		public Model1Types Type { get; set; }
	}
}
