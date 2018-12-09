using System.Collections.Generic;
using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class BookAuthorsOutput : BookAuthors
	{
		[Description("Book written by these authors.")]
		public Book Book { get; set; }

		[Description("Authors who wrote the book")]
		public List<Author> Authors { get; set; }
	}
}