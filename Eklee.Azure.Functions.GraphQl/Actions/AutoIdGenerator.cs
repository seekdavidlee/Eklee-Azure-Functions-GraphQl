using System;
using System.ComponentModel.DataAnnotations;
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

		public Task<bool> Transform(ModelTransformArguments arguments)
		{
			bool transformed = false;
			if (arguments.Action != MutationActions.DeleteAll &&
				arguments.Action != MutationActions.Delete &&
				arguments.Action != MutationActions.Update &&
				arguments.Models.Count > 0)
			{
				var typeAccessor = TypeAccessor.Create(arguments.Models.First().GetType());
				var autoIdMembers = typeAccessor.GetMembers().Where(x => x.GetAttribute(typeof(AutoIdAttribute), false) != null).ToList();
				if (autoIdMembers.Count > 0)
				{
					foreach (var model in arguments.Models)
					{
						autoIdMembers.ForEach(member =>
						{
							var value = typeAccessor[model, member.Name];
							if (value is string key && key == Marker)
							{
								typeAccessor[model, member.Name] = Guid.NewGuid().ToString("N");
								transformed = true;
							}
						});
					}
				}
			}

			return Task.FromResult(transformed);
		}
	}
}
