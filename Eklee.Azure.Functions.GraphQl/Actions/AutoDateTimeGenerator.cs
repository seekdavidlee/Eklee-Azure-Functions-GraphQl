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
		public const string Marker = "@dateTime(";

		public int ExecutionOrder => 0;

		public Task<bool> Transform(ModelTransformArguments arguments)
		{
			bool transformed = false;
			if (arguments.Action != MutationActions.DeleteAll &&
				arguments.Action != MutationActions.Delete &&
				arguments.Models.Count > 0)
			{
				var typeAccessor = TypeAccessor.Create(arguments.Models.First().GetType());
				var dateTimeMembers = typeAccessor.GetMembers().Where(x => x.GetAttribute(typeof(AutoDateTimeAttribute), false) != null).ToList();

				foreach (var model in arguments.Models)
				{
					dateTimeMembers.ForEach(member =>
					{
						bool isDateTime = true; ;
						bool process = typeAccessor[model, member.Name] is DateTime value &&
							(value == null || value == DateTime.MinValue);

						if (!process)
						{
							isDateTime = false;
							process = typeAccessor[model, member.Name] is DateTimeOffset value1 &&
							(value1 == null || value1 == DateTimeOffset.MinValue);
						}

						if (process)
						{
							var att = (AutoDateTimeAttribute)member.GetAttribute(typeof(AutoDateTimeAttribute), false);

							switch (att.AutoDateTimeTypes)
							{
								case AutoDateTimeTypes.UtcNow:
									typeAccessor[model, member.Name] = isDateTime ? DateTime.UtcNow : DateTimeOffset.UtcNow;
									transformed = true;
									break;

								case AutoDateTimeTypes.UtcToday:
									typeAccessor[model, member.Name] = isDateTime ? DateTime.UtcNow.Date : DateTimeOffset.UtcNow.Date;
									transformed = true;
									break;
							}
						}
					});
				}
			}

			return Task.FromResult(transformed);
		}
	}
}
