﻿using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model5Friend
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of Model 1, The Friend.")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[ConnectionEdgeDestination]
		[Description("TheFriend")]
		public Model5 TheFriend { get; set; }
	}
}