using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class BookReview
	{
		[Key]
		[Description("Id of book review.")]
		public string Id { get; set; }

		[Description("Id of Reviewer")]
		public string ReviewerId { get; set; }

		[Description("Id of book")]
		public string BookId { get; set; }

		[Description("Commentary by reviewer")]
		public string Comments { get; set; }

		[Description("1-5 starts rating")]
		public int Stars { get; set; }

		[Description("Determines whether book review is currently active or disabled.")]
		public bool Active { get; set; }

		[Description("Determine when book review is written.")]
		public DateTime WrittenOn { get; set; }
	}

	public class BookReviewId
	{
		[Description("Id of book review.")]
		[Key]
		public string Id { get; set; }
	}
}
