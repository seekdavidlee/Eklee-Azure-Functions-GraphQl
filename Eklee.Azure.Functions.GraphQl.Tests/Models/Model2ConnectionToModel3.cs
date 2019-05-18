using Eklee.Azure.Functions.GraphQl.Connections;

namespace Eklee.Azure.Functions.GraphQl.Tests.Models
{
	public class Model2ConnectionToModel3
	{
		[ConnectionEdgeDestinationKey]
		public string Id { get; set; }

		public string Field1 { get; set; }

		[ConnectionEdgeDestination]
		public Model3 Model3 { get; set; }
	}
}
