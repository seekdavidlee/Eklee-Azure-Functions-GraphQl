using Eklee.Azure.Functions.GraphQl.Connections;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Tests.Actions
{
	public class MockModel2
	{
		public string Id { get; set; }

		public List<MockModel3> Model3List { get; set; }

		[ConnectionEdgeDestination]
		public MockModel4 SomeMock { get; set; }
	}
}