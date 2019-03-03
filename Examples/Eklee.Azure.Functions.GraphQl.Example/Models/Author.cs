using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	[Description("Author for the book.")]
	public class Author
	{
		[Key]
		[Description("id of author")]
		public string Id { get; set; }

		[Description("Name of the author.")]
		public string Name { get; set; }

		[Description("Home city of the author.")]
		public string HomeCity { get; set; }
	}
}