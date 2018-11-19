using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class InputBuilderFactory
	{
		private readonly IGraphQlRepository _graphQlRepository;

		public InputBuilderFactory(IGraphQlRepository graphQlRepository)
		{
			_graphQlRepository = graphQlRepository;
		}

		public ModelConventionInputBuilder<TSource> Create<TSource>(ObjectGraphType objectGraphType)
		{
			return new ModelConventionInputBuilder<TSource>(objectGraphType, _graphQlRepository);
		}
	}
}
