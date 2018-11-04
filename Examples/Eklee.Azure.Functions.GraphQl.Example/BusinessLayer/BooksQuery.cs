using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksQuery : ObjectGraphType<object>
    {
        public BooksQuery(BooksRepository booksRepository, IGraphQlCache graphQlCache)
        {
            Name = "Query";

            // Example 1: We are getting a single Book. You are defining the argument yourself to pass into the repository with context. There's no caching and paging support. This is what comes out-of-the-box.

            Field<BookType>("book_nocache",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the book" }),
                resolve: context => booksRepository.GetBook(context.GetArgument<string>("id")));

            // Example 2: We are getting a single Book. The argument to pass into the repository is defined by the Model with at least one property with the KeyAttribute.
            //            The work is done by the cache repository which will cache the book result for a specific time you have defined. There's no paging support.

            Field<BookType>("book",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the book" }),
                resolve: graphQlCache.ResolverWithCache(key => booksRepository.GetBook((string)key), 10));


            // Example 3: We are getting a list of Books based on an argument. You are defining the key to pass into the repository without having to use context directly.
            //            The cache repository which will cache the book result for a specific time you have defined. There's no paging support.

            Field<ListGraphType<BookType>>("books_category",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "category", Description = "category of the book" }),
                resolve: graphQlCache.ResolverWithCache(key => booksRepository.GetBooks((string)key), 10, "category"));

            // Example 4: We are getting a list of paged Books. Technically, you are able to get all books by using TotalCount, although there's already a default page limit of 10 items per page if you don't specify.
            //            There's no caching support.

            Connection<BookType>().Name("booksConnection_nocache")
                .ResolveAsync(async context => await context.GetConnectionAsync(booksRepository.GetBooks()));

            // Example 5: We are getting a list of paged Books with a argument to be passed in. You are defining the argument yourself to pass into the repository with context.
            //            Technically, you are able to get all books by using TotalCount, although there's already a default page limit of 10 if you don't specify. There's no caching support.

            Connection<BookType>().Name("books_categoryConnection_nocache")
                .Argument<NonNullGraphType<StringGraphType>>("category", "category of the book")
                .ResolveAsync(async context => await context.GetConnectionAsync(booksRepository.GetBooks(context.GetArgument<string>("category"))));

            // Example 6: We are getting a list of paged Books with a argument to be passed in. You are defining the key to pass into the repository without having to use context directly.
            //            The cache repository which will cache the book result for a specific time you have defined. You will get paged results with a default page limit of 10 items per page if you don't specify.

            Connection<BookType>().Name("books_categoryConnection")
                .Argument<NonNullGraphType<StringGraphType>>("category", "category of the book")
                .ResolveAsync(async context => await context.GetConnectionWithCacheAsync(graphQlCache, key => booksRepository.GetBooks((string)key), "category"));
        }
    }
}
