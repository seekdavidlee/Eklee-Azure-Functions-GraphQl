using Eklee.Azure.Functions.GraphQl.Queries;
using FastMember;
using System;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search.Filters
{
	public class NumericSearchFilter : ISearchFilter
	{
		public bool CanHandle(Comparisons comparison, Member member)
		{
			return member.Type == typeof(int) || 
				member.Type == typeof(double) || 
				member.Type == typeof(DateTime);
		}

		private string GetComparison(SearchFilterModel searchFilterModel)
		{
			switch (searchFilterModel.Comprison)
			{
				case Comparisons.Equal:
					return Constants.ODataEqual;

				case Comparisons.NotEqual:
					return Constants.ODataNotEqual;

				case Comparisons.GreaterThan:
					return Constants.ODataGreaterThan;

				case Comparisons.GreaterEqualThan:
					return Constants.ODataGreaterEqualThan;

				case Comparisons.LessThan:
					return Constants.ODataLessThan;

				case Comparisons.LessEqualThan:
					return Constants.ODataLessEqualThan;

				default:
					throw new NotImplementedException($"Comparison {searchFilterModel.Comprison} is not implemented or is invalid.");
			}
		}

		public string GetFilter(SearchFilterModel searchFilterModel, Member member)
		{
			return $"{member.Name} {GetComparison(searchFilterModel)} {searchFilterModel.Value}";
		}
	}
}
