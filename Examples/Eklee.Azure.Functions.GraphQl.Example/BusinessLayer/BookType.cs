using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BookType : ObjectGraphType<Book>
    {
        public BookType()
        {
            Name = "book";

            Field(x => x.Id).Description("Id of book.");
            Field(x => x.Name).Description("Name of book");
            Field(x => x.Category).Description("Category");
        }
    }
}
