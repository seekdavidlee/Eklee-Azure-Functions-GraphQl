using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model5
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

		[Connection]
		[Description("A Best Friend")]
		public Model5Friend BestFriend { get; set; }

		[Connection]
		[Description("A Close Friend")]
		public Model5Friend CloseFriend { get; set; }
	}
}