using Eklee.Azure.Functions.GraphQl.Repository;
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

		Task<bool> Transform(ModelTransformArguments arguments);
	}
}
