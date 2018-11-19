using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class BookId
	{
		[Key]
		public string Id { get; set; }
	}
}
