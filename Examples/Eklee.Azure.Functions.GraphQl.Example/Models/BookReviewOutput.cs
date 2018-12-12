using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class BookReviewOutput : BookReview
	{
		[Description("Book that is reviewed.")]
		public Book Book { get; set; }

		[Description("Person who reviewed the book.")]
		public Reviewer Reviewer { get; set; }
	}
}