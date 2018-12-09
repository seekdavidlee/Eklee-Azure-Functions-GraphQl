using System;
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
				.WithProperty(x => x.Id)
				.BuildWithSingleResult();

			// Example 2: We are getting a single Book. The argument to pass into the repository is defined by the Model with at least one property with the KeyAttribute.
			//            The work is done by the cache repository which will cache the book result for a specific time you have defined. There's no paging support.

			queryBuilderFactory.Create<Book>(this, "getBook")
				.WithCache(TimeSpan.FromSeconds(10))
				.WithKeys()
				.BuildWithSingleResult();

			// Example 3: We are getting a list of Books based on an argument. You are defining the key to pass into the repository without having to use context directly.
			//            The cache repository which will cache the book result for a specific time you have defined. There's no paging support.

			queryBuilderFactory.Create<Book>(this, "getBooksByCategory")
				.WithCache(TimeSpan.FromSeconds(10))
				.WithProperty(x => x.Category)
				.BuildWithListResult();

			// Example 4: We are getting a list of paged Books. Technically, you are able to get all books by using TotalCount, although there's already a default page limit of 10 items per page if you don't specify.
			//            There's no caching support.

			queryBuilderFactory.Create<Book>(this, "getPagedBooks")
				.WithPaging()
				.BuildWithListResult();

			queryBuilderFactory.Create<Book>(this, "getPagedBooksByCategory")
				.WithPaging()
				.WithProperty(x => x.Category, true)
				.BuildWithListResult();

			// Example 6: We are getting a list of paged Books with a argument to be passed in. You are defining the key to pass into the repository without having to use context directly.
			//            The cache repository which will cache the book result for a specific time you have defined. You will get paged results with a default page limit of 10 items per page if you don't specify.

			queryBuilderFactory.Create<Book>(this, "getCachedPagedBooksByCategory")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithProperty(x => x.Category)
				.BuildWithListResult();

			queryBuilderFactory.Create<BookAuthorsOutput>(this, "getBookAuthors")
				.WithPaging()
				.WithCache(TimeSpan.FromSeconds(10))
				.WithQueryParameter("bookCategory", "Category of the books ")
				.BuildWithListResult();
		}
	}
}
