using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model6Friend
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of Connection.")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[ConnectionEdgeDestination]
		[Description("TheFriend")]
		public Model6 TheFriend { get; set; }
	}
}