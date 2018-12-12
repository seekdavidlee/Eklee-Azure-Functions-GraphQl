using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryParameter
	{
		public ModelMember MemberModel { get; set; }
		public ContextValue ContextValue { get; set; }
		public Comparisons Comparison { get; set; }

		public bool HasContextValue { get; set; }
		public bool ValueEquals(object target)
		{
			return (MemberModel.IsOptional && ContextValue == null) ||
				   MemberModel.PathMemberValueEquals(target, ContextValue.Value);
		}
	}

	public class QueryStep
	{
		public QueryStep()
		{
			QueryParameters = new List<QueryParameter>();
		}

		public DateTime? Started { get; set; }
		public Action<QueryExecutionContext> ContextAction { get; set; }
		public Func<QueryExecutionContext, List<object>> Mapper { get; set; }
		public List<QueryParameter> QueryParameters { get; set; }
		public DateTime? Ended { get; set; }
	}
}