using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Tests.Models
{
	public class ModelWith3Connections
	{
		[Key]
		public string Id { get; set; }

		public string Field1 { get; set; }

		[Connection]
		public ModelWith3ConnectionsEdge Edge1 { get; set; }

		[Connection]
		public ModelWith3ConnectionsEdge Edge2 { get; set; }

		[Connection]
		public ModelWith3ConnectionsEdge Edge3 { get; set; }
	}
}
