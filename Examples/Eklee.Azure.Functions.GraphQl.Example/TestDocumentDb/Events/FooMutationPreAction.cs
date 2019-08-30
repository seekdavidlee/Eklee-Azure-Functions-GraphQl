using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb.Events
{
	public class FooMutationPreAction : FooBarBase, IMutationPreAction
	{
		private readonly ILogger _logger;

		public FooMutationPreAction(ILogger logger)
		{
			_logger = logger;
		}
		public Task TryHandlePreItem<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			_logger.LogInformation($"FooMutationPreAction {GetBody(mutationActionItem)}");

			return Task.CompletedTask;
		}
	}
}
