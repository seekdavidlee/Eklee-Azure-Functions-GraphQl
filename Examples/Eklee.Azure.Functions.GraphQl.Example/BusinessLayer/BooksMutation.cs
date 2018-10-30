using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksMutation : ObjectGraphType
    {
        public BooksMutation(BooksRepository booksRepository)
        {
            Name = "Mutation";

            Field<BookType>(
                "createBook",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<BookInputType>> { Name = "book" }
                ),
                resolve: context =>
                {
                    var book = context.GetArgument<Book>("book");
                    return booksRepository.AddBook(book);
                });
        }
    }
}
