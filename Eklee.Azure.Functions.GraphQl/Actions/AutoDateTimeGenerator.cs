using Eklee.Azure.Functions.GraphQl.Attributes;
using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Actions
{
	public class AutoDateTimeGenerator : IModelTransformer
	{
		public int ExecutionOrder => 0;

		public bool CanHandle(MutationActions action)
		{
			return action != MutationActions.DeleteAll &&
				action != MutationActions.Delete;
		}

		public Task TransformAsync(object item, TypeAccessor typeAccessor, IGraphRequestContext context)
		{
			var dateTimeMembers = typeAccessor.GetMembers().Where(x => x.GetAttribute(typeof(AutoDateTimeAttribute), false) != null).ToList();

			dateTimeMembers.ForEach(member =>
			{
				bool isDateTime = true; ;
				bool process = typeAccessor[item, member.Name] is DateTime value &&
					(value == null || value == DateTime.MinValue);

				if (!process)
				{
					isDateTime = false;
					process = typeAccessor[item, member.Name] is DateTimeOffset value1 &&
					(value1 == null || value1 == DateTimeOffset.MinValue);
				}

				if (process)
				{
					var att = (AutoDateTimeAttribute)member.GetAttribute(typeof(AutoDateTimeAttribute), false);

					switch (att.AutoDateTimeTypes)
					{
						case AutoDateTimeTypes.UtcNow:
							typeAccessor[item, member.Name] = isDateTime ? DateTime.UtcNow : DateTimeOffset.UtcNow;

							break;

						case AutoDateTimeTypes.UtcToday:
							typeAccessor[item, member.Name] = isDateTime ? DateTime.UtcNow.Date : DateTimeOffset.UtcNow.Date;

							break;
					}
				}
			});

			return Task.CompletedTask;
		}
	}
}
