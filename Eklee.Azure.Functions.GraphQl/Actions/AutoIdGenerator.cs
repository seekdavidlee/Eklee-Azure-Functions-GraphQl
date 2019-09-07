using System;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Attributes;
using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Actions
{
	public class AutoIdGenerator : IModelTransformer
	{
		public const string Marker = "@";

		public int ExecutionOrder => 0;

		public bool CanHandle(MutationActions action)
		{
			return action != MutationActions.DeleteAll &&
				action != MutationActions.Delete &&
				action != MutationActions.Update;
		}

		public Task TransformAsync(object item, TypeAccessor typeAccessor, IGraphRequestContext context)
		{
			var autoIdMembers = typeAccessor.GetMembers().Where(x => x.GetAttribute(typeof(AutoIdAttribute), false) != null).ToList();
			if (autoIdMembers.Count > 0)
			{
				autoIdMembers.ForEach(member =>
				{
					var value = typeAccessor[item, member.Name];
					if (value is string key && key == Marker)
					{
						typeAccessor[item, member.Name] = Guid.NewGuid().ToString("N");

					}
				});
			}

			return Task.CompletedTask;
		}
	}
}
