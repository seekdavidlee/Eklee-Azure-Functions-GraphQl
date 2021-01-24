using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Actions
{
	public class ModelTransformerProvider : IModelTransformerProvider
	{
		private readonly List<IModelTransformer> _modelTransformers;

		public ModelTransformerProvider(IEnumerable<IModelTransformer> modelKeyTransformers)
		{
			_modelTransformers = modelKeyTransformers.OrderBy(x => x.ExecutionOrder).ToList();
		}

		public async Task TransformAsync(ModelTransformArguments arguments)
		{
			if (arguments.Models != null && arguments.Models.Count > 0)
			{
				await ProcessListAsync(arguments.Models, arguments.Action, arguments.RequestContext, false);
			}
		}

		private async Task ProcessListAsync(List<object> items, MutationActions action, IGraphRequestContext context, bool forceHandle)
		{
			var typeAccessor = TypeAccessor.Create(items.First().GetType());
			var listMembers = typeAccessor.GetMembers().Where(x => x.IsList());
			var connectionEdgeMembers = typeAccessor.GetMembers().Where(x => x.GetAttribute(typeof(ConnectionEdgeDestinationAttribute), false) != null);

			foreach (var x in _modelTransformers)
			{
				if (forceHandle || x.CanHandle(action))
				{
					foreach (var item in items)
					{
						await x.TransformAsync(item, typeAccessor, context);

						foreach (var member in listMembers)
						{
							var value = typeAccessor[item, member.Name];
							if (value != null)
							{
								List<object> memberItems = new List<object>();
								var list = (IList)value;
								foreach (var item1 in list)
								{
									memberItems.Add(item1);
								}

								if (memberItems.Count > 0)
									await ProcessListAsync(memberItems, action, context, true);
							}
						}

						foreach (var member in connectionEdgeMembers)
						{
							var value = typeAccessor[item, member.Name];
							if (value != null)
								await ProcessListAsync(new List<object> { value }, action, context, true);
						}
					}
				}
			}
		}
	}
}
