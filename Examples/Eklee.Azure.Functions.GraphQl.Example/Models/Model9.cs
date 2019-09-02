using Eklee.Azure.Functions.GraphQl.Connections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model9
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[Connection]
		[Description("Model7ToModel8")]
		public List<EachModel10> EachModel10List { get; set; }
	}
}
