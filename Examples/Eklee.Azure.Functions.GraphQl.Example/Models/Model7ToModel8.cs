using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model7ToModel8
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of destination")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[ConnectionEdgeDestination]
		[Description("TheModel8")]
		public Model8 TheModel8 { get; set; }
	}
}
