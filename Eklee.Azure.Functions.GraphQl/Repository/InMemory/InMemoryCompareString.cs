using System;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.InMemory
{
	public class InMemoryCompareString : IInMemoryCompare
	{
		public bool CanHandle(object obj, QueryParameter queryParameter)
		{
			var x = queryParameter.MemberModel.TypeAccessor[obj, queryParameter.MemberModel.Member.Name];
			return x is string && queryParameter.ContextValue.Comparison.HasValue &&
				queryParameter.ContextValue.GetFirstValue() is string;
		}

		public bool MeetsCondition(object obj, QueryParameter queryParameter)
		{
			string xStr = (string)queryParameter.MemberModel.TypeAccessor[obj, queryParameter.MemberModel.Member.Name];
			string ctxValueStr = (string)queryParameter.ContextValue.GetFirstValue();

			switch (queryParameter.ContextValue.Comparison)
			{
				case Comparisons.Equal:
					if (queryParameter.ContextValue.IsSingleValue())
						return xStr == ctxValueStr;
					else
					{
						return queryParameter.ContextValue.Values.Any(v => (string)v == xStr);
					}

				case Comparisons.StringContains:
					return xStr.Contains(ctxValueStr);

				case Comparisons.StringStartsWith:
					return xStr.StartsWith(ctxValueStr);

				case Comparisons.StringEndsWith:
					return xStr.EndsWith(ctxValueStr);

				default:
					throw new NotImplementedException($"String comparison {queryParameter.ContextValue.Comparison} is not implemented by InMemoryCompareString.");
			}
		}
	}
}
