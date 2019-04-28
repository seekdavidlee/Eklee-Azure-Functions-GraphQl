using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public interface IConnectionEdgeHandler
	{
		Task RemoveEdgeConnections(object item, IGraphRequestContext graphRequestContext);
		Task QueryAsync(List<object> results, QueryStep queryStep, IGraphRequestContext graphRequestContext);
	}
}
