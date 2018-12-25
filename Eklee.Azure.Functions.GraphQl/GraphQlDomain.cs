using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class GraphQlDomain : IGraphQlDomain
	{
		private readonly ISchema _schema;
		private readonly IDocumentExecuter _documentExecuter;
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;

		public GraphQlDomain(ISchema schema, IDocumentExecuter documentExecuter, IConfiguration configuration, ILogger logger)
		{
			_schema = schema;
			_documentExecuter = documentExecuter;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(GraphQlDomainRequest graphQlDomainRequest)
		{
			bool.TryParse(_configuration["GraphQl:EnableMetrics"], out var enableMetrics);

			var start = DateTime.UtcNow;

			var results = await _documentExecuter.ExecuteAsync(new ExecutionOptions
			{
				OperationName = graphQlDomainRequest.OperationName,
				Schema = _schema,
				Query = graphQlDomainRequest.Query,
				EnableMetrics = enableMetrics,
			});

			if (results.Errors != null && results.Errors.Count > 0)
			{
				results.Errors.ToList().ForEach(ex =>
				{
					_logger.LogError(ex.Message);
				});
			}

			if (enableMetrics)
				results.EnrichWithApolloTracing(start);

			return results;
		}
	}
}