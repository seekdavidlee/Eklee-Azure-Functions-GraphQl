using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class EachModel10
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of destination")]
		public string Id { get; set; }

		[Description("Field")]
		public string FieldDescription { get; set; }

		[ConnectionEdgeDestination]
		[Description("TheModel8")]
		public Model10 Model10 { get; set; }
	}
}
