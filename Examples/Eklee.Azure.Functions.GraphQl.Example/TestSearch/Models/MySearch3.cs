using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch.Models
{
	public class MySearch3
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }


		[Description("IntField")]
		public string IntField { get; set; }


		[Description("DoubleField")]
		public string DoubleField { get; set; }


		[Description("DateField")]
		public DateTime DateField { get; set; }


		[Description("Field")]
		public string Field { get; set; }
	}
}