using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl
{
    public class GraphQlDomain : IGraphQlDomain
    {
        private readonly ISchema _schema;
        private readonly IDocumentExecuter _documentExecuter;
        private readonly IConfiguration _configuration;

        public GraphQlDomain(ISchema schema, IDocumentExecuter documentExecuter, IConfiguration configuration)
        {
            _schema = schema;
            _documentExecuter = documentExecuter;
            _configuration = configuration;
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
                EnableMetrics = enableMetrics
            });

            if (enableMetrics)
                results.EnrichWithApolloTracing(start);

            return results;
        }

        public async Task<ExecutionResult> ExecuteAsync(string query)
        {
            return await _documentExecuter.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Query = query
            });
        }
    }
}