using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IGraphQlRepositoryProvider : IGraphQlRepository
	{
		void Use<TType, TRepository>(Dictionary<string, string> configurations = null) where TRepository : IGraphQlRepository;
		Task<IEnumerable<object>> QueryAsync(IEnumerable<QueryParameter> queryParameters);
	}
}
