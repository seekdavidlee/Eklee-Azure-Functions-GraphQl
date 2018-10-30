using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksSchema : Schema
    {
        public BooksSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<BooksQuery>();
            Mutation = resolver.Resolve<BooksMutation>();
        }
    }
}
