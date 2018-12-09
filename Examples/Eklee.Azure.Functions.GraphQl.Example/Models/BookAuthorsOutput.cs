using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class BookAuthorsOutput
	{
		[Key]
		[Description("Id of the book-authors")]
		public string Id { get; set; }

		[Description("Book written by these authors.")]
		public Book Book { get; set; }

		[Description("Authors who wrote the book")]
		public List<Author> Authors { get; set; }

		[Description("The type of royalty from book publishers.")]
		public string RoyaltyType { get; set; }
	}
}