using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionType<TSourceType> : ObjectGraphType<TSourceType>
	{
		public ModelConventionType()
		{
			this.AddFields();
		}
	}
}
