using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdgeDestinationFilter
	{
		public string Type { get; set; }

		public ModelMember ModelMember { get; set; }

		public Func<QueryExecutionContext, List<object>> Mapper { get; set; }
	}
}
