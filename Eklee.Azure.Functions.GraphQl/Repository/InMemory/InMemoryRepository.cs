using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository.InMemory
{
	public class InMemoryRepository : IGraphQlRepository
	{
		public InMemoryRepository(IInMemoryComparerProvider inMemoryComparerProvider)
		{
			_inMemoryComparerProvider = inMemoryComparerProvider;
		}

		private readonly Dictionary<string, Dictionary<string, object>> _database = new Dictionary<string, Dictionary<string, object>>();
		private readonly IInMemoryComparerProvider _inMemoryComparerProvider;

		private Dictionary<string, object> GetCollection<T>()
		{
			var key = typeof(T).FullName;

			if (_database.ContainsKey(key ?? throw new InvalidOperationException())) return _database[key];

			_database.Add(key, new Dictionary<string, object>());

			return _database[key];
		}

		public void Configure(Type sourceType, Dictionary<string, object> configurations)
		{
			// Do nothing.
		}

		public Task BatchAddAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			foreach (var item in items)
			{
				collection.Add(item.GetKey(), item);
			}
			return Task.CompletedTask;
		}

		public Task AddAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection.Add(item.GetKey(), item);
			return Task.CompletedTask;
		}

		public Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection[item.GetKey()] = item;
			return Task.CompletedTask;
		}

		public Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection.Remove(item.GetKey());
			return Task.CompletedTask;
		}

		public Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters, Dictionary<string, object> stepBagItems, IGraphRequestContext graphRequestContext) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			var parameters = queryParameters.ToList();
			if (parameters.Count > 0)
			{
				return Task.FromResult(_inMemoryComparerProvider.Query(queryParameters, collection.Values.ToList()).Select(x => (T)x));
			}

			return Task.FromResult(collection.Values.Select(x => (T)x).AsEnumerable());
		}

		public Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection.Clear();
			return Task.CompletedTask;
		}

		public Task AddOrUpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection[item.GetKey()] = item;
			return Task.CompletedTask;
		}

		public Task BatchAddOrUpdateAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			foreach (var item in items)
			{
				collection[item.GetKey()] = item;
			}
			return Task.CompletedTask;
		}
	}
}