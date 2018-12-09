using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputType<TSourceType> : InputObjectGraphType<TSourceType>
	{
		public ModelConventionInputType()
		{
			this.AddFields();
		}
	}
}
