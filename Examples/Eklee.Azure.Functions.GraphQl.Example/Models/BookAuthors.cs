using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class BookAuthors
	{
		[Key]
		[Description("Id of the book-authors")]
		public string Id { get; set; }

		[Description("Id of the book.")]
		[Edge(typeof(Book))]
		public string BookId { get; set; }

		[Description("Author Id list.")]
		[Edges(typeof(Author))]
		public List<string> AuthorIdList { get; set; }

		[Description("The type of royalty from book publishers.")]
		public string RoyaltyType { get; set; }
	}
}