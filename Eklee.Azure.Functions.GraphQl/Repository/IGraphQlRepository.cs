using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IGraphQlRepository
	{
		void Configure(Type sourceType, Dictionary<string, object> configurations);
		Task BatchAddAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class;
		Task BatchAddOrUpdateAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class;
		Task AddAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class;
		Task AddOrUpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class;
		Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class;
		Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class;
		Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters, Dictionary<string, object> stepBagItems, IGraphRequestContext graphRequestContext) where T : class;
		Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class;
	}
}
