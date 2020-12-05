using GraphQL;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public interface IContextValueResolver
	{
		ContextValue GetContextValue(IResolveFieldContext<object> context, ModelMember modelMember, ContextValueSetRule rule);
	}
}
