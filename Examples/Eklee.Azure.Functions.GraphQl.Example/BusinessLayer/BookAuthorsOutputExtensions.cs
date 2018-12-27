using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Example.Models;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public static class BookAuthorsOutputExtensions
	{
		public static void AddBookAuthorsOutputQueries(this BooksQuery booksQuery, QueryBuilderFactory queryBuilderFactory)
		{
			queryBuilderFactory.Create<BookAuthorsOutput>(booksQuery, "getBookAuthorsByCategory")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
					// We need to get all book-authors by book category. In order to perform this, we need to get all books with the category first. 
					.BeginWithProperty<Book>(x => x.Category, ctx =>
					{
						// Temporary store books in storage.
						ctx.Items["books"] = ctx.GetQueryResults<Book>();
					})
					// Then, we can get all book authors what has the book ids.
					.ThenWithProperty<BookAuthors>(x => x.BookId, ctx => ctx.GetItems<Book>("books").Select(y => (object)y.Id).ToList(), ctx =>
					{
						// Map initial result.
						var bookAuthors = ctx.GetQueryResults<BookAuthors>();
						var books = ctx.GetItems<Book>("books");

						ctx.SetResults(bookAuthors.Select(ba => new BookAuthorsOutput
						{
							Id = ba.Id,
							Book = books.Single(o => o.Id == ba.BookId),
							AuthorIdList = ba.AuthorIdList,
							BookId = ba.BookId,
							RoyaltyType = ba.RoyaltyType
						}).ToList());
					})
					// Lastly, we can get all the authors for all the found books.
					.ThenWithProperty<Author>(x => x.Id, ctx =>
					{
						var list = new List<string>();
						ctx.GetResults<BookAuthorsOutput>().ForEach(y => list.AddRange(y.AuthorIdList));
						return list.Distinct().Select(y => (object)y).ToList();
					}, ctx =>
					{
						// Map authors to result.
						var authors = ctx.GetQueryResults<Author>();

						// Only include authors who are in the AuthorIdList in each individual book author.
						ctx.GetResults<BookAuthorsOutput>().ForEach(ba => ba.Authors = authors.Where(x => ba.AuthorIdList.Contains(x.Id)).ToList());
					})
					.BuildQuery()
				.BuildWithListResult();

			queryBuilderFactory.Create<BookAuthorsOutput>(booksQuery, "getBookAuthorsById")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
				.BeginWithProperty<BookAuthors>(x => x.Id, ctx =>
				 {
					 ctx.SetResults(ctx.GetQueryResults<BookAuthors>().Select(ba => new BookAuthorsOutput
					 {
						 Id = ba.Id,
						 AuthorIdList = ba.AuthorIdList,
						 BookId = ba.BookId,
						 RoyaltyType = ba.RoyaltyType
					 }).ToList());
				 })
				.ThenWithProperty<Author>(x => x.Id, ctx =>
				 {
					 var list = new List<string>();
					 ctx.GetResults<BookAuthorsOutput>().ForEach(y => list.AddRange(y.AuthorIdList));
					 return list.Distinct().Select(y => (object)y).ToList();
				 }, ctx =>
				 {
					 // Map authors to result.
					 var authors = ctx.GetQueryResults<Author>();

					 // Only include authors who are in the AuthorIdList in each individual book author.
					 ctx.GetResults<BookAuthorsOutput>().ForEach(ba => ba.Authors = authors.Where(x => ba.AuthorIdList.Contains(x.Id)).ToList());
				 })
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<BookAuthorsOutput>(booksQuery, "getBookAuthorsByRoyaltyType")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
				.BeginWithProperty<BookAuthors>(x => x.RoyaltyType, ctx =>
				{
					ctx.SetResults(ctx.GetQueryResults<BookAuthors>().Select(ba => new BookAuthorsOutput
					{
						Id = ba.Id,
						AuthorIdList = ba.AuthorIdList,
						BookId = ba.BookId,
						RoyaltyType = ba.RoyaltyType
					}).ToList());
				})
				.ThenWithProperty<Author>(x => x.Id, ctx =>
				{
					var list = new List<string>();
					ctx.GetResults<BookAuthorsOutput>().ForEach(y => list.AddRange(y.AuthorIdList));
					return list.Distinct().Select(y => (object)y).ToList();
				}, ctx =>
				{
					 // Map authors to result.
					 var authors = ctx.GetQueryResults<Author>();

					 // Only include authors who are in the AuthorIdList in each individual book author.
					 ctx.GetResults<BookAuthorsOutput>().ForEach(ba => ba.Authors = authors.Where(x => ba.AuthorIdList.Contains(x.Id)).ToList());
				})
				.BuildQuery()
				.BuildWithListResult();
		}
	}
}
