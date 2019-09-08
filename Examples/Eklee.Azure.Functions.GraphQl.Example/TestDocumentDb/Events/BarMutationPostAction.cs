using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb.Events
{
	public class BarMutationPostAction : FooBarBase, IMutationPostAction
	{
		private readonly ILogger _logger;

		public BarMutationPostAction(ILogger logger)
		{
			_logger = logger;
		}

		public int ExecutionOrder => 1;

		public Task TryHandlePostItem<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			_logger.LogInformation($"BarMutationPostAction {GetBody(mutationActionItem)}");

			return Task.CompletedTask;
		}
	}
}
