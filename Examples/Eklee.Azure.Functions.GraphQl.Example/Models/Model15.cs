using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model15
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[Description("DateTimeOffsetField ")]
		public DateTimeOffset DateTimeOffsetField { get; set; }

		[Description("DateTimeOffsetField 2")]
		public DateTimeOffset? DateTimeOffsetField2 { get; set; }
	}
}
