using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IGraphQlRepositoryProvider
	{
		IGraphQlRepository Use<TType, TRepository>() where TRepository : IGraphQlRepository;
		Task<IEnumerable<object>> QueryAsync(IEnumerable<QueryParameter> queryParameters);
		IGraphQlRepository GetRepository<TRepository>();
	}
}
