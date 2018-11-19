using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class BookIdInputType : InputObjectGraphType<BookId>
	{
		public BookIdInputType()
		{
			Name = "BookIdInput";

			Field(x => x.Id).Description("Id of book");
		}
	}
}