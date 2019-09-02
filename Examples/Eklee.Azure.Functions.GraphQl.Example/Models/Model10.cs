using Eklee.Azure.Functions.GraphQl.Connections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model10
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[Connection]
		[Description("Model11")]
		public List<EachModel11> EachModel11List { get; set; }
	}
}
