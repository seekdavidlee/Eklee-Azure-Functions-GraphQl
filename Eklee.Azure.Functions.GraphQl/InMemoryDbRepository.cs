using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl
{
	public class InMemoryDbRepository : IGraphQlRepository
	{
		private readonly Dictionary<string, Dictionary<string, object>> _database = new Dictionary<string, Dictionary<string, object>>();

		public Task AddAsync<T>(T item)
		{
			var key = typeof(T).FullName;

			Dictionary<string, object> collection;

			// ReSharper disable once AssignNullToNotNullAttribute
			if (_database.ContainsKey(key))
			{
				collection = _database[key];
			}
			else
			{
				collection = new Dictionary<string, object>();
				_database.Add(key, collection);
			}

			collection.Add(item.GetKey(), item);
			return Task.CompletedTask;
		}

		public Task UpdateAsync<T>(T item)
		{
			var key = typeof(T).FullName;
			if (_database.ContainsKey(key))
			{
				var collection = _database[key];
				collection[item.GetKey()] = item;
			}

			return Task.CompletedTask;
		}

		public Task DeleteAsync<T>(T item)
		{
			var key = typeof(T).FullName;
			if (_database.ContainsKey(key))
			{
				var collection = _database[key];
				collection.Remove(item.GetKey());
			}

			return Task.CompletedTask;
		}

		public Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters)
		{
			var key = typeof(T).FullName;

			// ReSharper disable once AssignNullToNotNullAttribute
			if (_database.ContainsKey(key))
			{
				var collection = _database[key];
				var parameters = queryParameters.ToList();
				if (parameters.Count > 0)
				{
					var list = collection.Values.Where(x =>
						parameters.Count(queryParameter => queryParameter.ValueEquals(x)) == parameters.Count)
						.Select(x => (T)x);

					return Task.FromResult(list);
				}

				return Task.FromResult(collection.Values.Select(x => (T)x).AsEnumerable());
			}

			throw new ArgumentException($"Instance of Type {key} has not been added to the in-memory database.");
		}
	}
}