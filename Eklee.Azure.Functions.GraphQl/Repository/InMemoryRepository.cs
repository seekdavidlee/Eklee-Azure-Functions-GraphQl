using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class InMemoryRepository : IGraphQlRepository
	{
		private readonly Dictionary<string, Dictionary<string, object>> _database = new Dictionary<string, Dictionary<string, object>>();

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

		public Task BatchAddAsync<T>(IEnumerable<T> items) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			foreach (var item in items)
			{
				collection.Add(item.GetKey(), item);
			}
			return Task.CompletedTask;
		}

		public Task AddAsync<T>(T item) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection.Add(item.GetKey(), item);
			return Task.CompletedTask;
		}

		public Task UpdateAsync<T>(T item) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection[item.GetKey()] = item;
			return Task.CompletedTask;
		}

		public Task DeleteAsync<T>(T item) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection.Remove(item.GetKey());
			return Task.CompletedTask;
		}

		public Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters, Dictionary<string, object> stepBagItems) where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			var parameters = queryParameters.ToList();
			if (parameters.Count > 0)
			{
				var list = collection.Values.Where(x =>
					parameters.Count(queryParameter =>
						AssertIfStrings(x, queryParameter) ||
						AssertIfIntegers(x, queryParameter)) == parameters.Count)
					.Select(x => (T)x);

				return Task.FromResult(list);
			}

			return Task.FromResult(collection.Values.Select(x => (T)x).AsEnumerable());
		}

		private bool AssertIfIntegers(object x, QueryParameter queryParameter)
		{
			if (x is int xStr && queryParameter.ContextValue.Comparison.HasValue &&
				queryParameter.ContextValue.Value is int ctxValueStr)
			{
				switch (queryParameter.ContextValue.Comparison)
				{
					case Comparisons.Equal:
						return xStr == ctxValueStr;

					case Comparisons.NotEqual:
						return xStr != ctxValueStr;

					case Comparisons.GreaterThan:
						return xStr > ctxValueStr;

					case Comparisons.GreaterEqualThan:
						return xStr >= ctxValueStr;

					case Comparisons.LessThan:
						return xStr < ctxValueStr;

					case Comparisons.LessEqualThan:
						return xStr <= ctxValueStr;

					default:
						throw new NotImplementedException($"Int comparison {queryParameter.ContextValue.Comparison} is not implemented by InMemoryRepository.");
				}
			}

			return false;
		}

		private bool AssertIfStrings(object obj, QueryParameter queryParameter)
		{
			var x = queryParameter.MemberModel.TypeAccessor[obj, queryParameter.MemberModel.Member.Name];
			//queryParameter.MemberModel.
			if (x is string xStr && queryParameter.ContextValue.Comparison.HasValue &&
				queryParameter.ContextValue.Value is string ctxValueStr)
			{
				switch (queryParameter.ContextValue.Comparison)
				{
					case Comparisons.Equal:
						return xStr == ctxValueStr;

					case Comparisons.StringContains:
						return xStr.Contains(ctxValueStr);

					case Comparisons.StringStartsWith:
						return xStr.StartsWith(ctxValueStr);

					case Comparisons.StringEndsWith:
						return xStr.EndsWith(ctxValueStr);

					default:
						throw new NotImplementedException($"String comparison {queryParameter.ContextValue.Comparison} is not implemented by InMemoryRepository.");
				}
			}
			return false;
		}

		public Task DeleteAllAsync<T>() where T : class
		{
			Dictionary<string, object> collection = GetCollection<T>();
			collection.Clear();
			return Task.CompletedTask;
		}
	}
}