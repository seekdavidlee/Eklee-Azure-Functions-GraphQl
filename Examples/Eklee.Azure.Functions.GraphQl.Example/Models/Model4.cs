﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model4
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

		[StringLength(10, MinimumLength = 2)]
		[Description("Field")]
		public string Field { get; set; }
	}
}