using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model16
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[Description("DateTime ")]
		public DateTime DateTime { get; set; }

		[Description("DateTime 2")]
		public DateTime? DateTime2 { get; set; }
	}
}
