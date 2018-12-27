using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class GraphQlRepositoryProvider : IGraphQlRepositoryProvider
	{
		private readonly IEnumerable<IGraphQlRepository> _graphQlRepositories;
		private readonly Dictionary<string, IGraphQlRepository> _repositories = new Dictionary<string, IGraphQlRepository>();

		public GraphQlRepositoryProvider(IEnumerable<IGraphQlRepository> graphQlRepositories)
		{
			_graphQlRepositories = graphQlRepositories;
		}

		public IGraphQlRepository Use<TType, TRepository>() where TRepository : IGraphQlRepository
		{
			var repositoryName = typeof(TRepository).Name;
			var typeSourceName = typeof(TType).FullName;

			if (!_repositories.ContainsKey(typeSourceName ?? throw new InvalidOperationException()))
			{
				var repo = _graphQlRepositories.Single(x => x.GetType().Name == repositoryName);
				_repositories.Add(typeSourceName, repo);
				return repo;
			}

			return _repositories[typeSourceName];
		}

		public async Task<IEnumerable<object>> QueryAsync(string queryName, IEnumerable<QueryParameter> queryParameters)
		{
			var list = queryParameters.ToList();

			// ReSharper disable once AssignNullToNotNullAttribute
			var repo = _repositories[list.First().MemberModel.SourceType.FullName];

			MethodInfo method = repo.GetType().GetMethod("QueryAsync");

			// ReSharper disable once PossibleNullReferenceException
			MethodInfo generic = method.MakeGenericMethod(list.First().MemberModel.SourceType);

			var task = (Task)generic.Invoke(repo, new object[] { queryName, list });

			await task.ConfigureAwait(false);

			var resultProperty = task.GetType().GetProperty("Result");

			// ReSharper disable once PossibleNullReferenceException
			return (IEnumerable<object>)resultProperty.GetValue(task);
		}

		public IGraphQlRepository GetRepository<T>()
		{
			return _repositories[typeof(T).FullName ?? throw new InvalidOperationException()];
		}
	}
}
