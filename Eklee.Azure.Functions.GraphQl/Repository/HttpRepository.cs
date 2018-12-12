using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class HttpRepository : IGraphQlRepository
	{
		private readonly HttpClient _httpClient = new HttpClient();

		private readonly Dictionary<string, HttpTypeConfiguration> _httpTypeConfigurations
			= new Dictionary<string, HttpTypeConfiguration>();

		private readonly Dictionary<string, Func<object, HttpResource>> _addTransforms =
			new Dictionary<string, Func<object, HttpResource>>();

		private readonly Dictionary<string, Func<object, HttpResource>> _updateTransforms =
			new Dictionary<string, Func<object, HttpResource>>();

		private readonly Dictionary<string, Func<object, HttpResource>> _deleteTransforms =
			new Dictionary<string, Func<object, HttpResource>>();

		public void Configure(Type sourceType, Dictionary<string, string> configurations)
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			_httpTypeConfigurations.Add(sourceType.FullName, new HttpTypeConfiguration
			{
				BaseUrl = configurations[Constants.BaseUrl]
			});
		}

		private HttpClient GetHttpClient<T>()
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			var configuration = _httpTypeConfigurations[typeof(T).FullName];
			_httpClient.BaseAddress = new Uri(configuration.BaseUrl);
			return _httpClient;
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items)
		{
			foreach (var item in items.ToList())
			{
				await AddAsync(item);
			}
		}

		public async Task AddAsync<T>(T item)
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			var resource = _addTransforms[typeof(T).FullName](item);
			await InternalSendAsync(item, resource);
		}

		public void AddTransform(Type sourceType, Func<object, HttpResource> transform)
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			_addTransforms.Add(sourceType.FullName, transform);
		}

		public void UpdateTransform(Type sourceType, Func<object, HttpResource> transform)
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			_updateTransforms.Add(sourceType.FullName, transform);
		}

		public void DeleteTransform(Type sourceType, Func<object, HttpResource> transform)
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			_deleteTransforms.Add(sourceType.FullName, transform);
		}

		private async Task InternalSendAsync<T>(T item, HttpResource resource)
		{
			var httpClient = GetHttpClient<T>();

			var content = resource.ContainsBody == false ? null :
				new StringContent(JsonConvert.SerializeObject(item),
					Encoding.UTF8, "application/json");

			var request = new HttpRequestMessage(resource.Method, resource.AppendUrl) { Content = content };
			var response = await httpClient.SendAsync(request);

			response.EnsureSuccessStatusCode();
		}

		public async Task UpdateAsync<T>(T item)
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			var resource = _updateTransforms[typeof(T).FullName](item);
			await InternalSendAsync(item, resource);
		}

		public async Task DeleteAsync<T>(T item)
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			var resource = _deleteTransforms[typeof(T).FullName](item);

			if (!resource.ContainsBody.HasValue) resource.ContainsBody = false;

			await InternalSendAsync(item, resource);
		}

		public Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters)
		{
			throw new NotImplementedException();
		}
	}
}
