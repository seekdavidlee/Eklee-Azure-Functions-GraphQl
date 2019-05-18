using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Tests.Models
{
	public class ModelWith3ConnectionsOther
	{
		[Key]
		public string Id { get; set; }

		public string Field1 { get; set; }
	}
}
