using GraphQL;
using GraphQL.Validation;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class ExecutionResultResponseExtension
	{
		public static ExecutionResultResponse ToExecutionResultResponse(this ExecutionResult executionResult)
		{
			var response = new ExecutionResultResponse();
			response.Data = executionResult.Data;

			if (executionResult.Errors != null)
			{
				response.Errors = executionResult.Errors.Select(x =>
				{
					string code = "";
					if (x is ValidationError e)
					{
						code = e.Number;
					}
					return new ExecutionErrorResponse { Code = code, Message = x.Message, InnerException = x.InnerException };
				}).ToList();
			}

			response.Extensions = executionResult.Extensions;
			return response;
		}
	}
}