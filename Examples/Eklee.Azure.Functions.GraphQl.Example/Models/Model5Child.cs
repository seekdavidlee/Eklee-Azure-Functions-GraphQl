﻿using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model5Child
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[Connection]
		[Description("Parent")]
		public Model5 Parent { get; set; }
	}
}