using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BookInputType : InputObjectGraphType<Book>
    {
        public BookInputType()
        {
            Name = "BookInput";
            Field(x => x.Category);
            Field(x => x.Id);
            Field(x => x.Name);
        }
    }
}