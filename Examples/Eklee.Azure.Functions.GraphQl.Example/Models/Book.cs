using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Book
	{
		[Key]
		[Description("id of book")]
		public string Id { get; set; }

		[Description("Name of the book.")]
		public string Name { get; set; }

		[Description("category of book")]
		public string Category { get; set; }
	}
}
