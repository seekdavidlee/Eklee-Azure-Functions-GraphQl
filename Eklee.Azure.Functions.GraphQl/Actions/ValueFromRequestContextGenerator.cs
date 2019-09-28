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

	public class ValueFromRequestContextGenerator : IModelTransformer
	{
		private readonly IEnumerable<IRequestContextValueExtractor> _requestContextValueExtractors;

		public ValueFromRequestContextGenerator(IEnumerable<IRequestContextValueExtractor> requestContextValueExtractors)
		{
			_requestContextValueExtractors = requestContextValueExtractors;
		}
		public int ExecutionOrder => 0;

		public bool CanHandle(MutationActions action)
		{
			return action != MutationActions.DeleteAll &&
				action != MutationActions.Delete &&
				action != MutationActions.Update;
		}

		public async Task TransformAsync(object item, TypeAccessor typeAccessor, IGraphRequestContext context)
		{
			var keyMembers = typeAccessor.GetMembers().Where(x => x.GetAttribute(typeof(RequestContextValueAttribute), true) != null).ToList();
			if (keyMembers.Count > 0)
			{
				foreach (var keyMember in keyMembers)
				{
					var value = typeAccessor[item, keyMember.Name];
					var attr = (RequestContextValueAttribute)keyMember.GetAttribute(typeof(RequestContextValueAttribute), true);
					var gen = _requestContextValueExtractors.SingleOrDefault(x => x.GetType() == attr.Type);
					if (gen != null)
					{
						typeAccessor[item, keyMember.Name] = await gen.GetValueAsync(context, keyMember);
					}
				}
			}
		}
	}
}
