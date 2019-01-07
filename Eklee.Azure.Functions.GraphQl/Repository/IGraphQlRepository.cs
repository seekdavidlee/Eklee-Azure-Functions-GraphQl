using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IGraphQlRepository
	{
		void Configure(Type sourceType, Dictionary<string, object> configurations);
		Task BatchAddAsync<T>(IEnumerable<T> items) where T : class;
		Task AddAsync<T>(T item) where T : class;
		Task UpdateAsync<T>(T item) where T : class;
		Task DeleteAsync<T>(T item) where T : class;
		Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters, Dictionary<string, object> stepBagItems) where T : class;
		Task DeleteAllAsync<T>() where T : class;
	}
}
