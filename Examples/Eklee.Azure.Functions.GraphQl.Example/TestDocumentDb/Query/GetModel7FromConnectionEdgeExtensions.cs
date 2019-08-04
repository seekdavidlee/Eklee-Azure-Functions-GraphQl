using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb.Query
{
	public static class GetModel7FromConnectionEdgeExtensions
	{
		public static void AddGetModel7FromConnectionEdge(this TestDocumentDbQuery current, QueryBuilderFactory queryBuilderFactory)
		{
			// This query is really only meant to demostrate the ability to query for connection edges from the node itself 
			// even if user's query does not include the connection model. This is achieved via WithSourceIdFromSource. 
			// Typically, the use case may be that the user is performing a search (which doesn't use connection model). This
			// means as a developer, you may want to query the connection edge yourself to figure out the child node hanging on
			// the connection model.
			queryBuilderFactory.Create<Model7>(current, "GetModel7FromConnectionEdge")
				.WithParameterBuilder()
				.BeginQuery<Model7>().WithProperty(x => x.Id)
				.BuildQueryResult(ctx =>
				{
					ctx.Items["idList"] = ctx.GetQueryResults<Model7>().Select(x => (object)x.Id).ToList();
				})
				.WithConnectionEdgeBuilder<Model7ToModel8>()
				.WithSourceIdFromSource(x => (List<object>)x.Items["idList"])
				.BuildConnectionEdgeParameters(ctx =>
				{
					var connectionEdges = ctx.GetResults<ConnectionEdge>();

				})
				.BuildQuery()
				.BuildWithSingleResult();
		}
	}
}
