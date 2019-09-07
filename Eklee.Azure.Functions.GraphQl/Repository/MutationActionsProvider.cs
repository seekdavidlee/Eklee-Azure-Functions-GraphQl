using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class MutationActionsProvider : IMutationActionsProvider
	{
		private readonly List<IMutationPostAction> _mutationPostActions;
		private readonly List<IMutationPreAction> _mutationPreActions;

		public MutationActionsProvider(
			IEnumerable<IMutationPostAction> mutationPostActions,
			IEnumerable<IMutationPreAction> mutationPreActions)
		{
			_mutationPostActions = mutationPostActions.OrderBy(x => x.ExecutionOrder).ToList();
			_mutationPreActions = mutationPreActions.OrderBy(x => x.ExecutionOrder).ToList();
		}

		public async Task HandlePostActions<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			foreach (var action in _mutationPostActions)
			{
				await action.TryHandlePostItem(mutationActionItem);
			}
		}

		public async Task HandlePreActions<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			foreach (var action in _mutationPreActions)
			{
				await action.TryHandlePreItem(mutationActionItem);
			}
		}
	}
}
