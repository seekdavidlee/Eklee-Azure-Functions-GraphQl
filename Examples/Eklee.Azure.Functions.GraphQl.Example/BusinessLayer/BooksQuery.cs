using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksQuery : ObjectGraphType<object>
    {
        public BooksQuery(BooksRepository booksRepository)
        {
            Name = "Query";

            Field<BookType>("book",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the book" }),
                resolve: context => booksRepository.GetBook(context.GetArgument<string>("id")));


            Field<ListGraphType<BookType>>("books",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "category", Description = "category of the book" }),
                resolve: context => booksRepository.GetBooks(context.GetArgument<string>("category")));

        }
    }
}
