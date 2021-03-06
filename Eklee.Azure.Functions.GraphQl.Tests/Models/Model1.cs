﻿using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Tests.Models
{
	public class Model1
	{
		[Key]
		public string Id { get; set; }

		public string Field1 { get; set; }

		public int Field2 { get; set; }

		[Connection]
		public Model1Friend BestFriend { get; set; }
	}
}
