using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb.Events
{
	public class FooMutationPostAction : FooBarBase, IMutationPostAction
	{
		private readonly ILogger _logger;

		public int ExecutionOrder => 1;

		public FooMutationPostAction(ILogger logger)
		{
			_logger = logger;
		}
		public Task TryHandlePostItem<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			_logger.LogInformation($"FooMutationPostAction {GetBody(mutationActionItem)}");

			return Task.CompletedTask;
		}
	}
}
