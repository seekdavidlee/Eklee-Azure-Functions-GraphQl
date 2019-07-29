using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Repository.InMemory
{
	public interface IInMemoryComparerProvider
	{
		List<object> Query(IEnumerable<QueryParameter> queryParameters, List<object> list);
	}
}
