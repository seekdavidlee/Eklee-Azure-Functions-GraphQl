using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDbWithPermissions
{
	public class TestDocumentDbWithPermissionsQuery : ObjectGraphType<object>
	{
		public TestDocumentDbWithPermissionsQuery(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Creating document db queries.");

			Name = "query";
		}
	}
}