using System.Threading.Tasks;
using GraphQL;

namespace Eklee.Azure.Functions.GraphQl
{
    public interface IGraphQlDomain
    {
        Task<ExecutionResult> ExecuteAsync(GraphQlDomainRequest graphQlDomainRequest);
    }
}
