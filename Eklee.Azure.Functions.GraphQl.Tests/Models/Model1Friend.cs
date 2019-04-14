using Eklee.Azure.Functions.GraphQl.Connections;

namespace Eklee.Azure.Functions.GraphQl.Tests.Models
{
	public class Model1Friend
	{
		[ConnectionEdgeDestinationKey]
		public string Id { get; set; }

		public string Field1 { get; set; }

		public int Field2 { get; set; }

		[ConnectionEdgeDestination]
		public Model1 Model1 { get; set; }
	}
}
