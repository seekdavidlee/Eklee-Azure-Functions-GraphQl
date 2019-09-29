using Eklee.Azure.Functions.GraphQl.Connections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model13Parent
	{
		[Key]
		[Description("Some Key")]
		public string SomeKey { get; set; }

		[Description("Some Descr")]
		public string Descr { get; set; }

		[Description("Account Id")]
		public string AccountId { get; set; }

		[Connection]
		[Description("Connection")]
		public List<Model13Edge> Edges { get; set; }
	}
}
