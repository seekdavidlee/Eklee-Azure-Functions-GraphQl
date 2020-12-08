[Main page](../README.md)

# Query introduction

Before creating your queries, be sure to have setup [mutation](Mutations.md) for your model type. 

The first step is to define a class for queries and associate it in your schema defination. Notice that you will need to inject the helper query builder class, QueryBuilderFactory. We will also need to provide a name. By convention, we can use the word query.
```
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Examples
{
	public class MyQuery : ObjectGraphType<object>
	{
		public MyQuery(QueryBuilderFactory queryBuilderFactory)
		{
			Name = "query";
```

Now, you can apply query class defination in your schema defination.
```
using GraphQL;
using GraphQL.Types;

namespace Eklee.Examples
{
    public class MySchema : Schema
    {
        public BooksSchema(IDependencyResolver resolver, MyQuery myQuery, MyMutation myMutation) : base(resolver)
        {
            Query = myQuery;
```

Lastly, please note to register MyQuery in your AutoFac Module as part of your dependency injection setup.

## Simple Query by Id

This example shows how we can quickly setup a query by a field Id by starting off with the Create<T> extension method where T is the model type. In this context, it is a Book model.
```
queryBuilderFactory.Create<Book>(myQuery, "GetBookById")
	.WithParameterBuilder()
		.WithKeys()
		.BuildQuery()
	.BuildWithSingleResult();
```

There are a few things to note here. We pass in the MyQuery instance as well as a name of the query.

After that, we are ready to start with defining the query parameters. This is where we start with the WithParameterBuilder extension method which gives us the ability to invoke the extension method WithKeys. As long as we have define the Key attribute on the model property, we will know how to use that property for performing a query based on Id.

Lastly, we are ready to build the query with BuildQuery extension method and close off by stating that we are only expect one result from the query using BuildWithSingleResult.

Let's take a look at a different example. This is equivalent to the following except we are the one defining the property in which to query on. This applies to any property on the model type.
```
queryBuilderFactory.Create<Book>(myQuery, "GetBookById")
	.WithParameterBuilder()
		.WithProperty(x => x.Id)
		.BuildQuery()
	.BuildWithSingleResult();
```

## Query with Cache enabled

In this example, we use a WithCache extension method and provide a TimeSpan to define the cache time. Internally, we are using the entity's query parameter keys and values to determine the uniqueness of the query and cache the results.
```
queryBuilderFactory.Create<Book>(booksQuery, "getBook")
	.WithCache(TimeSpan.FromSeconds(10))
		.WithParameterBuilder()
		.WithKeys()
		.BuildQuery()
	.BuildWithSingleResult();
```

## Query with Paging enabled

**Starting from version 0.30, we have started using version 3.x of GraphQL.NET. GraphQL.NET has a new implementation for Connections which broke the ability to use a generic Typed class to wrap around a Model. Paging has been disabled because this incompatibility. The documentation below is for pre 0.30 versions.**

In this example, we use a WithPaging extension method. As part of the query on the client side, you will see paging specific paging parameters. Note that by default, the paging limit is 10. We can pass in a different paging limit into WithPaging to change that.
```
queryBuilderFactory.Create<Book>(booksQuery, "getPagedBooksByCategory")
	.WithPaging()
	.WithParameterBuilder()
		.WithProperty(x => x.Category, true)
		.BuildQuery()
	.BuildWithListResult();
```

The following is an example of a query we may execute on the client side. In the first example, a default limit of 10 records will be returned.
```
query {
  books_categoryConnection(category:"Art"){
    totalCount
    edges {
      cursor, node{
        id
        name
        category
      }
    }
    items {
      id
      name
      category
    }
    pageInfo {
      startCursor
      endCursor
      hasNextPage
      hasPreviousPage
    }
  }
}
```

In the next example, we define the return of the first 20 records.
```
query {
  books_categoryConnection(category:"Art", first: 20){
    totalCount
    edges {
      cursor, node{
        id
        name
        category
      }
    }
    items {
      id
      name
      category
    }
    pageInfo {
      startCursor
      endCursor
      hasNextPage
      hasPreviousPage
    }
  }
}
```

Here we are asking for the first 5 records after cursor "MA==" which in our case, represents the first record. Thus, we will NOT see the first record. Instead we will see the next 5 records after the first.
```
query {
  books_categoryConnection(category:"Art", first: 5, after: "MA=="){
    totalCount
    edges {
      cursor, node{
        id
        name
        category
      }
    }
    items {
      id
      name
      category
    }
    pageInfo {
      startCursor
      endCursor
      hasNextPage
      hasPreviousPage
    }
  }
}
```

## Query associated model types
Now, we are ready to show where GraphQL really shines. The following is an example of a BookReviewOutput model type with Book and Reviewer as properties. Both Book and Review are also classes of their own. Thus we are dealing with associations/relationships between model types.

We will need to start somewhere for the query. It really depends on the business context of where we would start the query from. For our example, let's assume our users have some idea of the book's name and category. From here, they wish to find out all the book reviews and their reviewers. In addition, our users wish to filter by the number of Stars from the review and the date in which the Review was written. Finally, we wish to only pull active reviews per our business rules.

With this context in mind, we can begin by using the BeginQuery<T>. This allows us to start a query from Book.
```
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
```

The result can be captured with the BuildQueryResult extension method. Here, we have the opportunity to store the book results in a temporary location within the context of the query execution context.

Next, we can use the ThenWithQuery<T> extension method to provide a query path to BookReview which our our association entity. Here, we can match by a list of Book Ids as well as any additional filter query parameters provided by the client side which include stars, written on and only active reviews.

Again, the BuildQueryResult extension method allows us to review the results and provide the first results of BookReviewOutput. We can use the SetResults method to set the BookReviewOutput result. Now, we are just missing the reviewer information.

We continue to use the ThenWithQuery<T> extension method to provide a query path to Reviewer where we have a list of Reviewer Ids. BuildQueryResult allow us to associate the results. We use the GetResults to pull the BookReviewOutput instance.

Finally, we close the query by using the BuildQuery and BuildWithListResult extension methods.

## Search

Once search type has been configured in mutations, we can simple use BeginSearch to search across one or more search types. This will give flexibility in query against multiple domain areas and using the association technique as described above to get matches orchestrated.

```
.BeginSearch(typeof(BookSearch), typeof(ReviewerSearch))
```