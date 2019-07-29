using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.InMemory
{
	public class InMemoryComparerProvider : IInMemoryComparerProvider
	{
		private readonly IEnumerable<IInMemoryCompare> _inMemoryCompares;

		public InMemoryComparerProvider(IEnumerable<IInMemoryCompare> inMemoryCompares)
		{
			_inMemoryCompares = inMemoryCompares;
		}

		public List<object> Query(IEnumerable<QueryParameter> queryParameters, List<object> list)
		{
			var parameters = queryParameters.ToList();
			return list.Where(x => parameters.Count(queryParameter =>
			{
				var comparer = _inMemoryCompares.SingleOrDefault(c => c.CanHandle(x, queryParameter));
				return comparer != null && comparer.MeetsCondition(x, queryParameter);
			}) == parameters.Count).ToList();
		}
	}
}
