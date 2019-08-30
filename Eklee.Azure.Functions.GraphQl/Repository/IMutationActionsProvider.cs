using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IMutationActionsProvider
	{
		Task HandlePreActions<TSource>(MutationActionItem<TSource> mutationActionItem);

		Task HandlePostActions<TSource>(MutationActionItem<TSource> mutationActionItem);
	}
}
