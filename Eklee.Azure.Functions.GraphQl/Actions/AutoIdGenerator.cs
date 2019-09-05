using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Actions.AutoIdPatterns;
using Eklee.Azure.Functions.GraphQl.Attributes;
using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Actions
{
	internal class AutoIdSelection
	{
		public Member Member { get; set; }
		public AutoIdAttribute Attribute { get; set; }
	}

	public class AutoIdGenerator : IMutationPreAction
	{
		private readonly IEnumerable<IAutoIdPattern> _autoIdPatterns;
		public const string Marker = "@autoId()";
		public int ExecutionOrder => 0;

		public AutoIdGenerator(IEnumerable<IAutoIdPattern> autoIdPatterns)
		{
			_autoIdPatterns = autoIdPatterns;
		}

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
				var withAutoIdMembers = typeAccessor.GetMembers().Select(x =>
				{
					var attr = x.GetAttribute(typeof(AutoIdAttribute), false) as AutoIdAttribute;
					if (attr != null)
					{
						return new AutoIdSelection { Attribute = attr, Member = x };
					}
					return null;

				}).Where(x => x != null).ToList();

				if (withAutoIdMembers.Count > 0)
				{
					items.ForEach(item =>
					{
						withAutoIdMembers.ForEach(a =>
						{
							var gen = _autoIdPatterns.SingleOrDefault(x => x.GetType() == a.Attribute.Type);
							if (gen != null)
							{
								if ((string)typeAccessor[item, a.Member.Name] == Marker)
									typeAccessor[item, a.Member.Name] = gen.Generate(item, a.Member);
							}
						});
					});
				}
			}

			return Task.CompletedTask;
		}

	}
}
