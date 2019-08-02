using System;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.InMemory
{
	public class InMemoryCompareDateTime : IInMemoryCompare
	{
		public bool CanHandle(object obj, QueryParameter queryParameter)
		{
			var x = queryParameter.MemberModel.TypeAccessor[obj, queryParameter.MemberModel.Member.Name];
			return x is DateTime && queryParameter.ContextValue.Comparison.HasValue &&
				queryParameter.ContextValue.GetFirstValue() is DateTime;
		}

		public bool MeetsCondition(object obj, QueryParameter queryParameter)
		{
			DateTime xDateTime = (DateTime)queryParameter.MemberModel.TypeAccessor[obj, queryParameter.MemberModel.Member.Name];
			DateTime ctxValueDateTime = (DateTime)queryParameter.ContextValue.GetFirstValue();

			switch (queryParameter.ContextValue.Comparison)
			{
				case Comparisons.Equal:
					if (queryParameter.ContextValue.IsSingleValue())
						return xDateTime == ctxValueDateTime;
					else
					{
						return queryParameter.ContextValue.Values.Any(v => (DateTime)v == xDateTime);
					}

				case Comparisons.NotEqual:
					return xDateTime != ctxValueDateTime;

				case Comparisons.GreaterThan:
					return xDateTime > ctxValueDateTime;

				case Comparisons.GreaterEqualThan:
					return xDateTime >= ctxValueDateTime;

				case Comparisons.LessThan:
					return xDateTime < ctxValueDateTime;

				case Comparisons.LessEqualThan:
					return xDateTime <= ctxValueDateTime;

				default:
					throw new NotImplementedException($"DateTime comparison {queryParameter.ContextValue.Comparison} is not implemented by InMemoryCompareDateTime.");
			}
		}
	}
}
