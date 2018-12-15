using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class DocumentDbRepository : IGraphQlRepository
	{
		public void Configure(Type sourceType, Dictionary<string, string> configurations)
		{
			throw new NotImplementedException();
		}

		public Task BatchAddAsync<T>(IEnumerable<T> items)
		{
			throw new NotImplementedException();
		}

		public Task AddAsync<T>(T item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync<T>(T item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync<T>(T item)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters)
		{
			throw new NotImplementedException();
		}
	}
}
