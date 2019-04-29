using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public interface IConnectionEdgeResolver
	{
		List<ConnectionEdge> HandleConnectionEdges<TSource>(List<TSource> items, Action<object> entityAction);
		List<ConnectionEdgeQueryParameter> ListConnectionEdgeQueryParameter(IEnumerable<object> items);
		List<ConnectionEdge> HandleConnectionEdges<TSource>(TSource item, Action<object> childAction);
	}
}
