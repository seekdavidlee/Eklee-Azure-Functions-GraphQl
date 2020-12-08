using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.Http;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Types;
using GraphQL.Validation;
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
		private readonly IEnumerable<IValidationRule> _validationRules;

		public GraphQlDomain(ISchema schema, IDocumentExecuter documentExecuter, IConfiguration configuration, ILogger logger,
			IHttpRequestContext requestContext,
			IGraphRequestContext graphRequestContext,
			IEnumerable<IValidationRule> validationRules)
		{
			_schema = schema;
			_documentExecuter = documentExecuter;
			_configuration = configuration;
			_logger = logger;
			_requestContext = requestContext;
			_graphRequestContext = graphRequestContext;
			_validationRules = validationRules;
		}

		public async Task<ExecutionResultResponse> ExecuteAsync(GraphQlDomainRequest graphQlDomainRequest)
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
				UserContext = _graphRequestContext.ToUserContext(),
				ThrowOnUnhandledException = !exposeExceptions,
				ValidationRules = DocumentValidator.CoreRules.Concat(_validationRules)
			};

			if (graphQlDomainRequest.Variables != null)
			{
				options.Inputs = JsonConvert.DeserializeObject<Inputs>(graphQlDomainRequest.Variables.ToString());
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

			// Using the default ExecutionResult would return a large amount of data back. Hence, we are controlling it with our own wrapper.
			return results.ToExecutionResultResponse();
		}
	}
}