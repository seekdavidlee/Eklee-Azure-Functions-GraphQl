using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class BookAuthorsQuery
	{
		[Description("Category of the books.")]
		public string BookCategory { get; set; }
	}
}