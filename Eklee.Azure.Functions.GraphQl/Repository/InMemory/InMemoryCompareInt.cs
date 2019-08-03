using System;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.InMemory
{
	public class InMemoryCompareInt : IInMemoryCompare
	{
		public bool CanHandle(object obj, QueryParameter queryParameter)
		{
			var x = queryParameter.MemberModel.TypeAccessor[obj, queryParameter.MemberModel.Member.Name];
			return x is int && queryParameter.ContextValue.Comparison.HasValue &&
				queryParameter.ContextValue.GetFirstValue() is int;
		}

		public bool MeetsCondition(object obj, QueryParameter queryParameter)
		{
			int xInt = (int)queryParameter.MemberModel.TypeAccessor[obj, queryParameter.MemberModel.Member.Name];
			int ctxValueInt = (int)queryParameter.ContextValue.GetFirstValue();

			switch (queryParameter.ContextValue.Comparison)
			{
				case Comparisons.Equal:
					if (queryParameter.ContextValue.IsSingleValue())
						return xInt == ctxValueInt;
					else
					{
						return queryParameter.ContextValue.Values.Any(v => (int)v == xInt);
					}

				case Comparisons.NotEqual:
					return xInt != ctxValueInt;

				case Comparisons.GreaterThan:
					return xInt > ctxValueInt;

				case Comparisons.GreaterEqualThan:
					return xInt >= ctxValueInt;

				case Comparisons.LessThan:
					return xInt < ctxValueInt;

				case Comparisons.LessEqualThan:
					return xInt <= ctxValueInt;

				default:
					throw new NotImplementedException($"Int comparison {queryParameter.ContextValue.Comparison} is not implemented by InMemoryCompareInt.");
			}
		}
	}
}
