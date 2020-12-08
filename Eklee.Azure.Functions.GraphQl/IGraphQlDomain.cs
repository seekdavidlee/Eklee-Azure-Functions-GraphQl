using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl
{
	public interface IGraphQlDomain
    {
        Task<ExecutionResultResponse> ExecuteAsync(GraphQlDomainRequest graphQlDomainRequest);
    }
}
