using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Tests.Models
{
	public class NoneConnectionModel
	{
		[Key]
		public string Id { get; set; }

		public string Field1 { get; set; }

		public int Field2 { get; set; }
	}
}
