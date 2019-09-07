using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Actions
{
	public class ModelTransformArguments
	{
		public List<object> Models { get; set; }
		public MutationActions Action { get; set; }
		public IGraphRequestContext RequestContext { get; set; }
	}

	public interface IModelTransformer
	{
		int ExecutionOrder { get; }

		bool CanHandle(MutationActions action);

		Task TransformAsync(object item, TypeAccessor typeAccessor, IGraphRequestContext context);
	}
}
