using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
    public class GraphQlDomain : IGraphQlDomain
    {
        private readonly ISchema _schema;
        private readonly IDocumentExecuter _documentExecuter;

        public GraphQlDomain(ISchema schema, IDocumentExecuter documentExecuter)
        {
            _schema = schema;
            _documentExecuter = documentExecuter;
        }

        public async Task<ExecutionResult> ExecuteAsync(GraphQlDomainRequest graphQlDomainRequest)
        {
            return await _documentExecuter.ExecuteAsync(new ExecutionOptions
            {
                OperationName = graphQlDomainRequest.OperationName,
                Schema = _schema,
                Query = graphQlDomainRequest.Query
            });
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