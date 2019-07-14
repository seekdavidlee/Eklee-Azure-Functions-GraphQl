using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryStep
	{
		public QueryStep()
		{
			QueryParameters = new List<QueryParameter>();
		}

		public DateTime? Started { get; set; }

		[JsonIgnore]
		public Action<QueryExecutionContext> ContextAction { get; set; }

		[JsonIgnore]
		public Func<QueryExecutionContext, List<object>> Mapper { get; set; }

		public List<QueryParameter> QueryParameters { get; set; }
		public DateTime? Ended { get; set; }

		[JsonIgnore]
		public Dictionary<string, object> Items { get; set; }
		public bool ForceCreateContextValueIfNull { get; set; }
	}
}