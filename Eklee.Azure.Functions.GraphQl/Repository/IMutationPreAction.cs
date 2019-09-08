using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IMutationPreAction
	{
		Task TryHandlePreItem<TSource>(MutationActionItem<TSource> mutationActionItem);

		int ExecutionOrder { get; }
	}
}
