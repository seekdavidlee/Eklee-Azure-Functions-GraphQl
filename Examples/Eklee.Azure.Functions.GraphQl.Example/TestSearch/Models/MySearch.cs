using Microsoft.Azure.Search;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch.Models
{
	public class MySearch
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

		[IsFacetable]
		[Description("Field")]
		public string Field { get; set; }

		[IsFacetable]
		[Description("FieldTwo")]
		public string FieldTwo { get; set; }

		[IsFacetable]
		[Description("FieldThree")]
		public string FieldThree { get; set; }
	}
}