using Eklee.Azure.Functions.GraphQl.Repository;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb.Events
{
	public abstract class FooBarBase
	{
		protected string GetBody<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			if (mutationActionItem.Item != null)
			{
				return JsonConvert.SerializeObject(mutationActionItem.Item);
			}

			if (mutationActionItem.Items != null)
			{
				return JsonConvert.SerializeObject(mutationActionItem.Items);
			}

			if (mutationActionItem.ObjectItem != null)
			{
				return JsonConvert.SerializeObject(mutationActionItem.ObjectItem);
			}

			if (mutationActionItem.ObjectItems != null)
			{
				return JsonConvert.SerializeObject(mutationActionItem.ObjectItems);
			}

			return "";
		}
	}
}
