using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class InMemoryDbRepository : IGraphQlRepository
	{
		private readonly Dictionary<string, Dictionary<string, object>> _database = new Dictionary<string, Dictionary<string, object>>();

		private Dictionary<string, object> GetCollection<T>()
		{
			var key = typeof(T).FullName;

			if (_database.ContainsKey(key ?? throw new InvalidOperationException())) return _database[key];

			_database.Add(key, new Dictionary<string, object>());

			return _database[key];
		}

		public Task BatchAddAsync<T>(IEnumerable<T> items)
		{
			Dictionary<string, object> collection = GetCollection<T>();
			foreach (var item in items)
			{
				collection.Add(item.GetKey(), item);
			}
			return Task.CompletedTask;
		}

		public Task AddAsync<T>(T item)
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection.Add(item.GetKey(), item);
			return Task.CompletedTask;
		}

		public Task UpdateAsync<T>(T item)
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection[item.GetKey()] = item;
			return Task.CompletedTask;
		}

		public Task DeleteAsync<T>(T item)
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection.Remove(item.GetKey());
			return Task.CompletedTask;
		}

		public Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters)
		{
			Dictionary<string, object> collection = GetCollection<T>();
			var parameters = queryParameters.ToList();
			if (parameters.Count > 0)
			{
				var list = collection.Values.Where(x =>
					parameters.Count(queryParameter => queryParameter.Comparison == Comparisons.Equals && queryParameter.ValueEquals(x)) == parameters.Count)
					.Select(x => (T)x);

				return Task.FromResult(list);
			}

			return Task.FromResult(collection.Values.Select(x => (T)x).AsEnumerable());
		}
	}
}