using Eklee.Azure.Functions.GraphQl.Connections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model7
	{
		[Key]
		[Description("Id")]
		public string Id { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[Connection]
		[Description("Model7ToModel8")]
		public Model7ToModel8 Model7ToModel8 { get; set; }
	}
}
