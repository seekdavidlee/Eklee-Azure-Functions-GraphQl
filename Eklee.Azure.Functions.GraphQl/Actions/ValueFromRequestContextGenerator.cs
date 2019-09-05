using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Actions.RequestContextValueExtractors;
using Eklee.Azure.Functions.GraphQl.Attributes;
using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Actions
{
	internal class RequestContextValueSelection
	{
		public Member Member { get; set; }
		public RequestContextValueAttribute Attribute { get; set; }
	}

	public class ValueFromRequestContextGenerator : IMutationPreAction
	{
		private readonly IEnumerable<IRequestContextValueExtractor> _requestContextValueExtractors;

		public ValueFromRequestContextGenerator(IEnumerable<IRequestContextValueExtractor> requestContextValueExtractors)
		{
			_requestContextValueExtractors = requestContextValueExtractors;
		}
		public int ExecutionOrder => 0;

		public Task TryHandlePreItem<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			Type type = null;
			List<object> items = null;

			if (mutationActionItem.Action == MutationActions.BatchCreateOrUpdate ||
				mutationActionItem.Action == MutationActions.BatchCreate)
			{
				if (mutationActionItem.Items != null)
				{
					type = mutationActionItem.Items.First().GetType();
					items = mutationActionItem.Items.Select(x => (object)x).ToList();
				}
				else
				{
					type = mutationActionItem.ObjectItems.First().GetType();
					items = mutationActionItem.ObjectItems;
				}
			}

			if (mutationActionItem.Action == MutationActions.Create ||
				mutationActionItem.Action == MutationActions.CreateOrUpdate)
			{
				items = new List<object>();
				if (mutationActionItem.Item != null)
				{
					type = mutationActionItem.Item.GetType();
					items.Add(mutationActionItem.Item);
				}
				else
				{
					type = mutationActionItem.ObjectItem.GetType();
					items.Add(mutationActionItem.ObjectItem);
				}
			}

			if (type != null && items != null && items.Count > 0)
			{
				var typeAccessor = TypeAccessor.Create(type);
				var members = typeAccessor.GetMembers().Select(x =>
				{
					var attr = x.GetAttribute(typeof(RequestContextValueAttribute), false) as RequestContextValueAttribute;
					if (attr != null)
					{
						return new RequestContextValueSelection { Attribute = attr, Member = x };
					}
					return null;

				}).Where(x => x != null).ToList();

				if (members.Count > 0)
				{
					items.ForEach(item =>
					{
						members.ForEach(a =>
						{
							var gen = _requestContextValueExtractors.SingleOrDefault(x => x.GetType() == a.Attribute.Type);
							if (gen != null)
							{
								typeAccessor[item, a.Member.Name] = gen.GetValue(mutationActionItem.RequestContext, a.Member);
							}
						});
					});
				}
			}

			return Task.CompletedTask;
		}
	}
}
