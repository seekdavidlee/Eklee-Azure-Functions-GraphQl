using Microsoft.Azure.Search;
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

		[IsFilterable]
		[IsFacetable]
		[Description("IntField")]
		public int IntField { get; set; }

		[IsFilterable]
		[IsFacetable]
		[Description("DoubleField")]
		public double DoubleField { get; set; }

		[IsFilterable]
		[IsFacetable]
		[Description("DateField")]
		public DateTime DateField { get; set; }

		[IsFilterable]
		[IsFacetable]
		[Description("Field")]
		public string Field { get; set; }

		[IsFilterable]
		[IsFacetable]
		[Description("FieldTwo")]
		public string FieldTwo { get; set; }

		[IsFilterable]
		[IsFacetable]
		[Description("FieldThree")]
		public string FieldThree { get; set; }
	}
}