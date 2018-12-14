using Eklee.Azure.Functions.GraphQl.Repository;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class InputBuilderFactory
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ILogger _logger;

		public InputBuilderFactory(IGraphQlRepositoryProvider graphQlRepositoryProvider, ILogger logger)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_logger = logger;
		}

		public ModelConventionInputBuilder<TSource> Create<TSource>(ObjectGraphType objectGraphType)
		{
			return new ModelConventionInputBuilder<TSource>(objectGraphType, _graphQlRepositoryProvider, _logger);
		}
	}
}
