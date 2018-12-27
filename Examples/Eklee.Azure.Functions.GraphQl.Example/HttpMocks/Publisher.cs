using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.HttpMocks
{
	public class Publisher
	{
		[Key]
		[Description("Publisher Id.")]
		public string Id { get; set; }

		[Description("Name of publisher.")]
		public string Name { get; set; }
	}
}
