using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdge
	{
		[Key]
		[Description("Id of ConnectionEdge.")]
		public string Id { get; set; }

		[Description("The source entity's field name.")]
		public string SourceFieldName { get; set; }

		[Description("The source entity's type.")]
		public string SourceType { get; set; }

		[Description("The source entity's Id.")]
		public string SourceId { get; set; }

		[Description("The destination entity's Id.")]
		public string DestinationId { get; set; }

		[Description("The destination field name.")]
		[Obsolete("This is no longer required. It remains for backwards compatibility.")]
		public string DestinationFieldName { get; set; }

		[Description("The stored object as JSON.")]
		public string MetaValue { get; set; }

		[Description("The stored object's type.")]
		public string MetaType { get; set; }

		[Description("The stored object's field name.")]
		public string MetaFieldName { get; set; }
	}
}
