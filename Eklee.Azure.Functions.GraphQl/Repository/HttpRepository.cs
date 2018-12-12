using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class HttpRepository : IGraphQlRepository
	{
		private readonly HttpClient _httpClient = new HttpClient();

		private string _baseUrl;

		public void Configure(Type sourceType, Dictionary<string, string> configurations)
		{
			_baseUrl = configurations[Constants.BaseUrl];
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

		public Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters)
		{
			throw new NotImplementedException();
		}
	}
}
