using Eklee.Azure.Functions.GraphQl.Attributes;
using Eklee.Azure.Functions.GraphQl.Example.Actions;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model12
	{
		[AutoId]
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[ModelField(false)]
		[Description("Field")]
		public string Field { get; set; }

		[AutoDateTime(AutoDateTimeTypes.UtcNow)]
		[ModelField(false)]
		[Description("FieldDateTimeNow")]
		public DateTimeOffset FieldDateTimeNow { get; set; }

		[AutoDateTime(AutoDateTimeTypes.UtcToday)]
		[ModelField(false)]
		[Description("FieldDateTimeToday")]
		public DateTime FieldDateTimeToday { get; set; }

		[ModelField(true)]
		[Description("FieldDateTime")]
		public DateTime FieldDateTime { get; set; }

		[RequestContextValue(typeof(ValueFromRequestHeader))]
		[ModelField(false)]
		[Description("FieldFromHeader")]
		public string FieldFromHeader { get; set; }

	}
}
