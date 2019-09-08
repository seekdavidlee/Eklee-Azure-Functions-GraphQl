using Eklee.Azure.Functions.GraphQl.Attributes;
using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2.Models
{
	public class MySearch4
	{
		[AutoId]
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[AutoDateTime(AutoDateTimeTypes.UtcToday)]
		[Description("DateField")]
		[ModelField(false)]
		public DateTime Created { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[IsFilterable]
		[IsFacetable]
		[Description("State")]
		public string State { get; set; }

		[Description("List5")]
		public List<MySearch5> List5 { get; set; }

		[Description("List6")]
		public List<MySearch6> List6 { get; set; }
	}
}
