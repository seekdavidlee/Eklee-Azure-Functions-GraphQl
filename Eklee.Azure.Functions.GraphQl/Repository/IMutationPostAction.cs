using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IMutationPostAction
	{
		Task TryHandlePostItem<TSource>(MutationActionItem<TSource> mutationActionItem);
	}
}
