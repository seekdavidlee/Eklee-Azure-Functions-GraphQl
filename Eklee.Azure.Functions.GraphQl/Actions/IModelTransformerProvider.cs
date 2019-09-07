using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Actions
{
	public interface IModelTransformerProvider
	{
		Task TransformAsync(ModelTransformArguments arguments);
	}
}
