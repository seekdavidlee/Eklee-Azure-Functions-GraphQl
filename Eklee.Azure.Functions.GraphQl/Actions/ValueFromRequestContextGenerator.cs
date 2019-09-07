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

		public async Task<bool> Transform(ModelTransformArguments arguments)
		{
			bool transformed = false;
			if (arguments.Action != MutationActions.DeleteAll &&
				arguments.Action != MutationActions.Delete &&
				arguments.Action != MutationActions.Update &&
				arguments.Models.Count > 0)
			{
				var typeAccessor = TypeAccessor.Create(arguments.Models.First().GetType());
				var keyMembers = typeAccessor.GetMembers().Where(x => x.GetAttribute(typeof(RequestContextValueAttribute), false) != null).ToList();
				if (keyMembers.Count > 0)
				{
					foreach (var model in arguments.Models)
					{
						foreach (var keyMember in keyMembers)
						{
							var value = typeAccessor[model, keyMember.Name];
							var attr = (RequestContextValueAttribute)keyMember.GetAttribute(typeof(RequestContextValueAttribute), false);
							var gen = _requestContextValueExtractors.SingleOrDefault(x => x.GetType() == attr.Type);
							if (gen != null)
							{
								typeAccessor[model, keyMember.Name] = await gen.GetValueAsync(arguments.RequestContext, keyMember);
								transformed = true;
							}
						}
					}
				}
			}

			return transformed;
		}
	}
}
