using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb.Events
{
	public class BarMutationPreAction : FooBarBase, IMutationPreAction
	{
		private readonly ILogger _logger;

		public int ExecutionOrder => 1;

		public BarMutationPreAction(ILogger logger)
		{
			_logger = logger;
		}
		public Task TryHandlePreItem<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			_logger.LogInformation($"BarMutationPreAction {GetBody(mutationActionItem)}");

			return Task.CompletedTask;
		}
	}
}
