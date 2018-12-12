using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class BooksQuery : ObjectGraphType<object>
	{
		public BooksQuery(QueryBuilderFactory queryBuilderFactory)
		{
			Name = "query";

			// Example 1: We are getting a single Book. You are defining the argument yourself to pass into the repository with context. There's no caching and paging support. This is what comes out-of-the-box.

			queryBuilderFactory.Create<Book>(this, "getBookNoCache")
				.WithParameterBuilder()
					.WithProperty(x => x.Id, Comparisons.Equals)
					.Build()
				.BuildWithSingleResult();

			// Example 2: We are getting a single Book. The argument to pass into the repository is defined by the Model with at least one property with the KeyAttribute.
			//            The work is done by the cache repository which will cache the book result for a specific time you have defined. There's no paging support.

			queryBuilderFactory.Create<Book>(this, "getBook")
				.WithCache(TimeSpan.FromSeconds(10))
					.WithParameterBuilder()
					.WithKeys()
					.Build()
				.BuildWithSingleResult();

			// Example 3: We are getting a list of Books based on an argument. You are defining the key to pass into the repository without having to use context directly.
			//            The cache repository which will cache the book result for a specific time you have defined. There's no paging support.

			queryBuilderFactory.Create<Book>(this, "getBooksByCategory")
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
					.WithProperty(x => x.Category, Comparisons.Equals)
					.Build()
				.BuildWithListResult();

			// Example 4: We are getting a list of paged Books. Technically, you are able to get all books by using TotalCount, although there's already a default page limit of 10 items per page if you don't specify.
			//            There's no caching support.

			queryBuilderFactory.Create<Book>(this, "getPagedBooks")
				.WithPaging()
				.BuildWithListResult();

			queryBuilderFactory.Create<Book>(this, "getPagedBooksByCategory")
				.WithPaging()
				.WithParameterBuilder()
					.WithProperty(x => x.Category, Comparisons.Equals, true)
					.Build()
				.BuildWithListResult();

			// Example 6: We are getting a list of paged Books with a argument to be passed in. You are defining the key to pass into the repository without having to use context directly.
			//            The cache repository which will cache the book result for a specific time you have defined. You will get paged results with a default page limit of 10 items per page if you don't specify.

			queryBuilderFactory.Create<Book>(this, "getCachedPagedBooksByCategory")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
					.WithProperty(x => x.Category, Comparisons.Equals)
					.Build()
				.BuildWithListResult();

			queryBuilderFactory.Create<BookAuthorsOutput>(this, "getBookAuthorsByCategory")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
					// We need to get all book-authors by book category. In order to perform this, we need to get all books with the category first. 
					.BeginWithProperty<Book>(x => x.Category, Comparisons.Equals, ctx =>
					{
						// Temporary store books in storage.
						ctx.Items["books"] = ctx.GetQueryResults<Book>();
					})
					// Then, we can get all book authors what has the book ids.
					.ThenWithProperty<BookAuthors>(x => x.BookId, Comparisons.Equals, ctx => ctx.GetItems<Book>("books").Select(y => (object)y.Id).ToList(), ctx =>
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
					.ThenWithProperty<Author>(x => x.Id, Comparisons.Equals, ctx =>
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
					.Build()
				.BuildWithListResult();

			queryBuilderFactory.Create<BookAuthorsOutput>(this, "getBookAuthorsById")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
				.BeginWithProperty<BookAuthors>(x => x.Id, Comparisons.Equals, ctx =>
				{
					ctx.SetResults(ctx.GetQueryResults<BookAuthors>().Select(ba => new BookAuthorsOutput
					{
						Id = ba.Id,
						AuthorIdList = ba.AuthorIdList,
						BookId = ba.BookId,
						RoyaltyType = ba.RoyaltyType
					}).ToList());
				})
				.ThenWithProperty<Author>(x => x.Id, Comparisons.Equals, ctx =>
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
				.Build()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<BookAuthorsOutput>(this, "getBookAuthorsByRoyaltyType")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
				.BeginWithProperty<BookAuthors>(x => x.RoyaltyType, Comparisons.Equals, ctx =>
				{
					ctx.SetResults(ctx.GetQueryResults<BookAuthors>().Select(ba => new BookAuthorsOutput
					{
						Id = ba.Id,
						AuthorIdList = ba.AuthorIdList,
						BookId = ba.BookId,
						RoyaltyType = ba.RoyaltyType
					}).ToList());
				})
				.ThenWithProperty<Author>(x => x.Id, Comparisons.Equals, ctx =>
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
				.Build()
				.BuildWithListResult();
		}
	}
}
