using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ExecutionResultResponse
	{
		public object Data { get; set; }

		public List<ExecutionErrorResponse> Errors { get; set; }

		public Dictionary<string, object> Extensions { get; set; }
	}
}