using System;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.Http;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl
{
	public class GraphQlDomain : IGraphQlDomain
	{
		private readonly ISchema _schema;
		private readonly IDocumentExecuter _documentExecuter;
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;
		private readonly IHttpRequestContext _requestContext;
		private readonly IGraphRequestContext _graphRequestContext;

		public GraphQlDomain(ISchema schema, IDocumentExecuter documentExecuter, IConfiguration configuration, ILogger logger,
			IHttpRequestContext requestContext,
			IGraphRequestContext graphRequestContext)
		{
			_schema = schema;
			_documentExecuter = documentExecuter;
			_configuration = configuration;
			_logger = logger;
			_requestContext = requestContext;
			_graphRequestContext = graphRequestContext;
		}

		public async Task<ExecutionResult> ExecuteAsync(GraphQlDomainRequest graphQlDomainRequest)
		{
			bool.TryParse(_configuration["GraphQl:EnableMetrics"], out var enableMetrics);
			bool.TryParse(_configuration["GraphQl:ExposeExceptions"], out var exposeExceptions);

			var start = DateTime.UtcNow;

			_graphRequestContext.HttpRequest = _requestContext;

			var options = new ExecutionOptions
			{
				OperationName = graphQlDomainRequest.OperationName,
				Schema = _schema,
				Query = graphQlDomainRequest.Query,
				EnableMetrics = enableMetrics,
				UserContext = _graphRequestContext,
				ExposeExceptions = exposeExceptions,
			};

			if (graphQlDomainRequest.Variables != null)
			{
				options.Inputs = JsonConvert.SerializeObject(graphQlDomainRequest.Variables).ToInputs();
			}

			var results = await _documentExecuter.ExecuteAsync(options);

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