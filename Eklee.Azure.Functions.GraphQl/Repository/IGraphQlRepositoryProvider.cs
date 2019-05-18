using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IGraphQlRepositoryProvider
	{
		IGraphQlRepository Use<TType, TRepository>() where TRepository : IGraphQlRepository;
		Task<IEnumerable<object>> QueryAsync(string queryName, QueryStep queryStep, IGraphRequestContext graphRequestContext);
		IGraphQlRepository GetRepository<TRepository>();
		IGraphQlRepository GetRepository(Type type);
		bool IsRepositoryExist<TRepository>();
		bool IsRepositoryExist(Type type);
	}
}
