using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class BookDeleteOutputType : ObjectGraphType<Status>
	{
		public BookDeleteOutputType()
		{
			Name = "status";

			Field(x => x.Message).Description("Status of the operation.");
		}
	}
}