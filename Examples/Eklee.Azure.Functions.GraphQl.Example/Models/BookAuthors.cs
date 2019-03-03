using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	[Description("Represents the relationship between a book and its authors.")]
	public class BookAuthors
	{
		[Key]
		[Description("Id of the book-authors")]
		public string Id { get; set; }

		[Description("Id of the book.")]
		public string BookId { get; set; }

		[Description("Author Id list.")]
		public List<string> AuthorIdList { get; set; }

		[Description("The type of royalty from book publishers.")]
		public string RoyaltyType { get; set; }
	}
}