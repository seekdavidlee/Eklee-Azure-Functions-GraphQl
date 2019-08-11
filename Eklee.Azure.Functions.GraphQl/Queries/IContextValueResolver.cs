using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public interface IContextValueResolver
	{
		ContextValue GetContextValue(ResolveFieldContext<object> context, ModelMember modelMember, ContextValueSetRule rule);
	}
}
