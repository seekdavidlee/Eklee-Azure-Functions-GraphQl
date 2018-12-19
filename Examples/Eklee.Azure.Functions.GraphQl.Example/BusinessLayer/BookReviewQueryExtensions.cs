using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Example.Models;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public static class BookReviewQueryExtensions
	{
		public static void AddBookReviewQueries(this BooksQuery booksQuery, QueryBuilderFactory queryBuilderFactory)
		{
			queryBuilderFactory.Create<BookReviewOutput>(booksQuery, "GetBookReviewByBookName")
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
					.BeginWithProperty<Book>(x => x.Name, Comparisons.Equals, ctx =>
					{
						// Temporary store books in storage.
						ctx.Items["books"] = ctx.GetQueryResults<Book>();
					})
				.ThenWithProperty<BookReview>(x => x.BookId, Comparisons.Equals,
					ctx => ctx.GetItems<Book>("books").Select(y => (object)y.Id).ToList(),
					ctx =>
					{
						var bookReviews = ctx.GetQueryResults<BookReview>();

						ctx.Items["reviewerIdList"] = bookReviews.Select(x => (object)x.ReviewerId).ToList();

						var books = ctx.GetItems<Book>("books");

						ctx.SetResults(bookReviews.Select(br => new BookReviewOutput
						{
							Id = br.Id,
							BookId = br.BookId,
							Comments = br.Comments,
							ReviewerId = br.ReviewerId,
							Stars = br.Stars,
							Book = books.Single(x => x.Id == br.BookId)
						}).ToList());
					})
				.ThenWithProperty<Reviewer>(x => x.Id, Comparisons.Equals, ctx => (List<object>)ctx.Items["reviewerIdList"],
					ctx =>
					{
						var reviewers = ctx.GetQueryResults<Reviewer>();
						ctx.GetResults<BookReviewOutput>().ForEach(x => x.Reviewer = reviewers.Single(y => y.Id == x.Id));
					})
				.Build()
				.BuildWithSingleResult();
		}
	}
}
