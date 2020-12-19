using Eklee.Azure.Functions.GraphQl.Attributes;
using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model6
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("IntField")]
		public int IntField { get; set; }

		[Description("DoubleField")]
		public double DoubleField { get; set; }

		[Description("DateField")]
		public DateTime DateField { get; set; }

		[Description("Field")]
		public string Field { get; set; }
		
		[ModelField(false)]
		[Description("Field2")]
		public string Field2 { get; set; }

		[ModelField(false)]
		[Description("Field3")]
		public string Field3 { get; set; }

		[Connection]
		[Description("List of Best Friends")]
		public List<Model6Friend> BestFriends { get; set; }
	}
}