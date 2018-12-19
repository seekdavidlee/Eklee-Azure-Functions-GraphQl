using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IGraphQlRepository
	{
		void Configure(Type sourceType, Dictionary<string, object> configurations);
		Task BatchAddAsync<T>(IEnumerable<T> items);
		Task AddAsync<T>(T item);
		Task UpdateAsync<T>(T item);
		Task DeleteAsync<T>(T item);
		Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters);
		Task DeleteAllAsync<T>();
	}
}
