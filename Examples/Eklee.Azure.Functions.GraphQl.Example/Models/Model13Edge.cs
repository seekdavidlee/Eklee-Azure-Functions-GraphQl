using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model13Edge
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of destination")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[ConnectionEdgeDestination]
		[Description("Child")]
		public Model13Child Child { get; set; }
	}
}
