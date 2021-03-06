﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class TestPermissionModel1
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Category")]
		public string Category { get; set; }

		[Description("Value")]
		public string Value { get; set; }
	}
}
