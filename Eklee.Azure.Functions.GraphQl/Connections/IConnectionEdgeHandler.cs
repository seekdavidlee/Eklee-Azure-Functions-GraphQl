using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public interface IConnectionEdgeHandler
	{
		Task QueryAsync(List<object> results, QueryStep queryStep, IGraphRequestContext graphRequestContext);
	}
}
