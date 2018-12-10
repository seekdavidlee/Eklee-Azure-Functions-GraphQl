using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class GraphQlRepository
	{
		public IGraphQlRepository Repository { get; set; }
		public Dictionary<string, string> Configurations { get; set; }
	}

	public class GraphQlRepositoryProvider : IGraphQlRepositoryProvider
	{
		private readonly IEnumerable<IGraphQlRepository> _graphQlRepositories;
		private readonly Dictionary<string, GraphQlRepository> _repositories = new Dictionary<string, GraphQlRepository>();

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
				_repositories.Add(typeSourceName, new GraphQlRepository
				{
					Repository = _graphQlRepositories.Single(x => x.GetType().Name == repositoryName),
					Configurations = configurations
				});
			}
		}

		public async Task<IEnumerable<object>> QueryAsync(IEnumerable<QueryParameter> queryParameters)
		{
			var list = queryParameters.ToList();

			// ReSharper disable once AssignNullToNotNullAttribute
			var repo = _repositories[list.First().MemberModel.SourceType.FullName].Repository;

			MethodInfo method = repo.GetType().GetMethod("QueryAsync");

			// ReSharper disable once PossibleNullReferenceException
			MethodInfo generic = method.MakeGenericMethod(list.First().MemberModel.SourceType);

			var task = (Task)generic.Invoke(repo, new object[] { list });

			await task.ConfigureAwait(false);

			var resultProperty = task.GetType().GetProperty("Result");

			// ReSharper disable once PossibleNullReferenceException
			return (IEnumerable<object>)resultProperty.GetValue(task);
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items)
		{
			await GetRepository<T>().BatchAddAsync(items);
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
