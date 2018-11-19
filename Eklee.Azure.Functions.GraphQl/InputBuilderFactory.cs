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

		public InputBuilder<TInputType, TOutputType, TSource> Create<TInputType, TOutputType, TSource>(ObjectGraphType objectGraphType) where TInputType : GraphType where TOutputType : IGraphType
		{
			return new InputBuilder<TInputType, TOutputType, TSource>(objectGraphType, _graphQlRepository);
		}

		public ModelConventionInputBuilder<TSource> Create<TSource>(ObjectGraphType objectGraphType)
		{
			return new ModelConventionInputBuilder<TSource>(objectGraphType, _graphQlRepository);
		}

		public void BuildWithModelConvention<TSource>(ObjectGraphType objectGraphType)
		{
			new ModelConventionInputBuilder<TSource>(objectGraphType, _graphQlRepository).Build();
		}
	}
}
