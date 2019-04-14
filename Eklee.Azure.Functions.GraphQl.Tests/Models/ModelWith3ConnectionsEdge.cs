using Eklee.Azure.Functions.GraphQl.Connections;

namespace Eklee.Azure.Functions.GraphQl.Tests.Models
{
	public class ModelWith3ConnectionsEdge
	{
		[ConnectionEdgeDestinationKey]
		public string Id { get; set; }

		public string Field1 { get; set; }

		[ConnectionEdgeDestination]
		public ModelWith3ConnectionsOther Other { get; set; }
	}
}
