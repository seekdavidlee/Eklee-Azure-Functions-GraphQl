using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl
{
	public class GraphRepository
	{
		public IGraphQlRepository Repository { get; set; }
		public Dictionary<string, string> Configurations { get; set; }
	}

	public class GraphQlRepositoryProvider : IGraphQlRepositoryProvider
	{
		private readonly IEnumerable<IGraphQlRepository> _graphQlRepositories;
		private readonly Dictionary<string, GraphRepository> _repositories = new Dictionary<string, GraphRepository>();

		public GraphQlRepositoryProvider(IEnumerable<IGraphQlRepository> graphQlRepositories)
		{
			_graphQlRepositories = graphQlRepositories;
		}

		public void Use<TType, TRepository>(Dictionary<string, string> configurations = null) where TRepository : IGraphQlRepository
		{
			var repositoryName = typeof(TRepository).Name;
			var typeSourceName = typeof(TType).FullName;

			if (!_repositories.ContainsKey(typeSourceName ?? throw new InvalidOperationException()))
			{
				_repositories.Add(typeSourceName, new GraphRepository
				{
					Repository = _graphQlRepositories.Single(x => x.GetType().Name == repositoryName),
					Configurations = configurations
				});
			}
		}

		public async Task AddAsync<T>(T item)
		{
			await GetRepository<T>().AddAsync(item);
		}

		public async Task UpdateAsync<T>(T item)
		{
			await GetRepository<T>().UpdateAsync(item);
		}

		public async Task DeleteAsync<T>(T item)
		{
			await GetRepository<T>().DeleteAsync(item);
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters)
		{
			return await GetRepository<T>().QueryAsync<T>(queryParameters);
		}

		private IGraphQlRepository GetRepository<T>()
		{
			return _repositories[typeof(T).FullName ?? throw new InvalidOperationException()].Repository;
		}
	}
}
