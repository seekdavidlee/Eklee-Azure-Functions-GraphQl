using GraphQL.Types;
using Microsoft.Extensions.Logging;
using Eklee.Azure.Functions.GraphQl.Example.Models;

namespace Eklee.Azure.Functions.GraphQl.Example.TestStorage.Core
{
	public class TestStorageQueryConfigObjectGraphType : ObjectGraphType<object>
	{
		public TestStorageQueryConfigObjectGraphType(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			Name = "query";

			queryBuilderFactory.Create<Model7>(this, "GetModel7WithModel8Id", "Get Model7")
				.WithParameterBuilder()
				.WithConnectionEdgeBuilder<Model7ToModel8>()
					.WithDestinationId()
				.BuildConnectionEdgeParameters()
				.BuildQuery()
				.BuildWithListResult();
		}
	}
}
