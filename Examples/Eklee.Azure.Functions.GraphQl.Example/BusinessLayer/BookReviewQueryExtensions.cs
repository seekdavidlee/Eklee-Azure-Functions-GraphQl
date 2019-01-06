using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using Eklee.Azure.Functions.GraphQl.Repository.Search;

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
					.BuildQueryResult(ctx =>
					{
						// Temporary store books in storage.
						ctx.Items["books"] = ctx.GetQueryResults<Book>();
					})
				.ThenWithQuery<BookReview>()
					.WithPropertyFromSource(x => x.BookId, ctx => ctx.GetItems<Book>("books").Select(y => (object)y.Id).ToList())
					.BuildQueryResult(ctx =>
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
					.BuildQueryResult(ctx =>
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
					.BuildQueryResult(ctx =>
					{
						// Temporary store books in storage.
						ctx.Items["books"] = ctx.GetQueryResults<Book>();
					})
				.ThenWithQuery<BookReview>() // Gives you the ability to query with book review stars, written on, active reviews and book Id matches.
					.WithPropertyFromSource(x => x.BookId, ctx => ctx.GetItems<Book>("books").Select(y => (object)y.Id).ToList())
					.WithProperty(x => x.Stars)
					.WithProperty(x => x.Active)
					.WithProperty(x => x.WrittenOn)
					.BuildQueryResult(ctx =>
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
					.BuildQueryResult(ctx =>
					{
						var reviewers = ctx.GetQueryResults<Reviewer>();
						ctx.GetResults<BookReviewOutput>().ForEach(
							x => x.Reviewer = reviewers.Single(y => y.Id == x.ReviewerId));
					})
				.BuildQuery()
				.BuildWithListResult();

			// Example of how we are leveraging searches across business domains, reviewers and books.
			queryBuilderFactory.Create<BookReviewOutput>(booksQuery, "SearchBookReviews")
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
				.BeginSearch(typeof(BookSearch), typeof(ReviewerSearch))
					.BuildQueryResult(ctx =>
					{
						var searches = ctx.GetQueryResults<SearchResultModel>();
						var bookSearches = searches.GetTypeList<BookSearch>();

						ctx.Items["bookSearches"] = bookSearches;
						ctx.Items["bookSearchesIdList"] = bookSearches.Select(x => x.Id).ToList();

						var reviewerSearches = searches.GetTypeList<ReviewerSearch>();

						ctx.Items["reviewerSearchesIdList"] = reviewerSearches.Select(x => x.Id).ToList();
					})
				.ThenWithQuery<BookReview>()
					.WithPropertyFromSource(x => x.BookId, ctx => ctx.ConvertItemsToObjectList<string>("bookSearchesIdList"))
					.BuildQueryResult(ctx =>
					{
						// Temporary store books in storage.
						ctx.Items["bookReviews"] = ctx.GetQueryResults<BookReview>();
					})
				.ThenWithQuery<BookReview>()
					.WithPropertyFromSource(x => x.ReviewerId, ctx => ctx.ConvertItemsToObjectList<string>("reviewerSearchesIdList"))
					.BuildQueryResult(ctx =>
					{
						// Combine the results of book reviews from book Id list and reviewer Id List.
						List<BookReview> bookReviews = (List<BookReview>)ctx.Items["bookReviews"];

						bookReviews.AddRange(ctx.GetQueryResults<BookReview>());

						var finalResults = bookReviews.Distinct().Select(br => new BookReviewOutput
						{
							Id = br.Id,
							BookId = br.BookId,
							Comments = br.Comments,
							ReviewerId = br.ReviewerId,
							Stars = br.Stars,
							WrittenOn = br.WrittenOn,
							Active = br.Active
						}).ToList();

						ctx.Items["bookIdList"] = finalResults.Select(x => x.BookId).Distinct().ToList();
						ctx.Items["reviewerIdList"] = finalResults.Select(x => x.ReviewerId).Distinct().ToList();

						ctx.SetResults(finalResults);
					})
				.ThenWithQuery<Book>()
					.WithPropertyFromSource(x => x.Id, ctx => ctx.ConvertItemsToObjectList<string>("bookIdList"))
					.BuildQueryResult(ctx =>
					{
						var books = ctx.GetQueryResults<Book>();
						ctx.GetResults<BookReviewOutput>().ForEach(x =>
							x.Book = books.Single(b => b.Id == x.BookId));
					})
				.ThenWithQuery<Reviewer>()
					.WithPropertyFromSource(x => x.Id, ctx => ctx.ConvertItemsToObjectList<string>("reviewerIdList"))
					.BuildQueryResult(ctx =>
					{
						var reviewers = ctx.GetQueryResults<Reviewer>();
						ctx.GetResults<BookReviewOutput>().ForEach(x =>
							x.Reviewer = reviewers.Single(b => b.Id == x.ReviewerId));
					})
				.BuildQuery()
				.BuildWithListResult();
		}
	}
}
