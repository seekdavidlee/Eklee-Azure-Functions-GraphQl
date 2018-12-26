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
				.BeginQuery<Book>()
					.WithProperty(x => x.Name)
					.BuildQuery(ctx =>
					{
						// Temporary store books in storage.
						ctx.Items["books"] = ctx.GetQueryResults<Book>();
					})
				.ThenWithQuery<BookReview>()
					.WithPropertyFromSource(x => x.BookId, ctx => ctx.GetItems<Book>("books").Select(y => (object)y.Id).ToList())
					.BuildQuery(ctx =>
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
				.ThenWithQuery<Reviewer>()
					.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["reviewerIdList"])
					.BuildQuery(ctx =>
					{
						var reviewers = ctx.GetQueryResults<Reviewer>();
						ctx.GetResults<BookReviewOutput>().ForEach(
							x => x.Reviewer = reviewers.Single(y => y.Id == x.ReviewerId));
					})
				.BuildQuery()
				.BuildWithListResult();


			queryBuilderFactory.Create<BookReviewOutput>(booksQuery, "GetBookReviewsWithBookNameAndCategory")
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
				.BeginQuery<Book>()  // Gives you the ability to query with both book name and category.
					.WithProperty(x => x.Name)
					.WithProperty(x => x.Category)
					.BuildQuery(ctx =>
					{
						// Temporary store books in storage.
						ctx.Items["books"] = ctx.GetQueryResults<Book>();
					})
				.ThenWithQuery<BookReview>() // Gives you the ability to query with both book review star and book Id matches.
					.WithPropertyFromSource(x => x.BookId, ctx => ctx.GetItems<Book>("books").Select(y => (object)y.Id).ToList())
					.WithProperty(x => x.Stars)
					.WithProperty(x => x.Active)
					.WithProperty(x => x.WrittenOn)
					.BuildQuery(ctx =>
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
							Book = books.Single(x => x.Id == br.BookId),
							WrittenOn = br.WrittenOn,
							Active = br.Active
						}).ToList());
					})
				.ThenWithQuery<Reviewer>()
					.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["reviewerIdList"])
					.BuildQuery(ctx =>
					{
						var reviewers = ctx.GetQueryResults<Reviewer>();
						ctx.GetResults<BookReviewOutput>().ForEach(
							x => x.Reviewer = reviewers.Single(y => y.Id == x.ReviewerId));
					})
				.BuildQuery()
				.BuildWithListResult();
		}
	}
}
