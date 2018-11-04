using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksQuery : ObjectGraphType<object>
    {
        public BooksQuery(BooksRepository booksRepository, IGraphQlCache graphQlCache)
        {
            Name = "Query";

            Field<BookType>("book",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the book" }),
                resolve: graphQlCache.ResolverWithCache(key => booksRepository.GetBook((string)key), 10));

            Field<ListGraphType<BookType>>("books",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "category", Description = "category of the book" }),
                resolve: graphQlCache.ResolverWithCache(key => booksRepository.GetBooks((string)key), 10, "category"));

        }
    }
}
