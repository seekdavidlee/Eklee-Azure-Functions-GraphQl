using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdge
	{
		[Key]
		public string Id { get; set; }

		public string SourceFieldName { get; set; }

		public string SourceType { get; set; }

		public string SourceId { get; set; }

		public string DestinationTypeName { get; set; }

		public string DestinationFieldName { get; set; }

		public string DestinationId { get; set; }

		public string MetaValue { get; set; }

		public string MetaType { get; set; }

		public string MetaFieldName { get; set; }
	}
}
