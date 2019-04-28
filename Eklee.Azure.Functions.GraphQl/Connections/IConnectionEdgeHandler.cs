using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public interface IConnectionEdgeHandler
	{
		Task DeleteAllEdgeConnectionsOfType<T>(IGraphRequestContext graphRequestContext);
		Task RemoveEdgeConnections(object item, IGraphRequestContext graphRequestContext);
		Task QueryAsync(List<object> results, QueryStep queryStep, IGraphRequestContext graphRequestContext);
	}
}
