using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl
{
	public interface IGraphQlRepository
	{
		Task BatchAddAsync<T>(IEnumerable<T> items);
		Task AddAsync<T>(T item);
		Task UpdateAsync<T>(T item);
		Task DeleteAsync<T>(T item);
		Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters);
	}
}
