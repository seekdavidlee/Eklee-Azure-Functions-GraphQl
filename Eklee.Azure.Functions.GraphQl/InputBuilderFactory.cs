using Eklee.Azure.Functions.GraphQl.Repository;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class InputBuilderFactory
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;

		public InputBuilderFactory(IGraphQlRepositoryProvider graphQlRepositoryProvider)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
		}

		public ModelConventionInputBuilder<TSource> Create<TSource>(ObjectGraphType objectGraphType)
		{
			return new ModelConventionInputBuilder<TSource>(objectGraphType, _graphQlRepositoryProvider);
		}
	}
}
