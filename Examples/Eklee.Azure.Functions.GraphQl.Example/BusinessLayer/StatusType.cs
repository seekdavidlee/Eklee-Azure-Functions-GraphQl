using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class StatusType : ObjectGraphType<Status>
	{
		public StatusType()
		{
			Name = "status";

			Field(x => x.Message).Description("Message describing the status of the operation");
		}
	}
}