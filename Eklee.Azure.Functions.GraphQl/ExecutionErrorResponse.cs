using System;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ExecutionErrorResponse
	{
		public string Code { get; set; }
		public string Message { get; set; }
		public Exception InnerException { get; set; }
	}
}