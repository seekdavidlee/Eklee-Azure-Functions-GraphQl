using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl
{
	public interface IGraphQlRepositoryProvider : IGraphQlRepository
	{
		void Use<TType, TRepository>(Dictionary<string, string> configurations = null) where TRepository : IGraphQlRepository;
	}
}
