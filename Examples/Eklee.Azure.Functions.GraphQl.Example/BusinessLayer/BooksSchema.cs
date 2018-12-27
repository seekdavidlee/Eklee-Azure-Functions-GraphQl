using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksSchema : Schema
    {
        public BooksSchema(IDependencyResolver resolver, BooksQuery booksQuery, BooksMutation booksMutation) : base(resolver)
        {
            Query = booksQuery;
            Mutation = booksMutation;
        }
    }
}
