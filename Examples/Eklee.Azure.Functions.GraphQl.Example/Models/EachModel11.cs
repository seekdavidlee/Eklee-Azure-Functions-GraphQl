using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class EachModel11
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of destination")]
		public string Id { get; set; }

		[Description("Field")]
		public string FieldDescription { get; set; }

		[ConnectionEdgeDestination]
		[Description("TheModel11")]
		public Model11 Model11 { get; set; }
	}
}
