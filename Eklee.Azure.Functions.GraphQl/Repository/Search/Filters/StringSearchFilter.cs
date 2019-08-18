using Eklee.Azure.Functions.GraphQl.Queries;
using FastMember;
using System;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search.Filters
{
	public class StringSearchFilter : ISearchFilter
	{
		public bool CanHandle(Comparisons comparison, Member member)
		{
			return member.Type == typeof(string);
		}

		public string GetFilter(SearchFilterModel searchFilterModel, Member member)
		{
			return $"{member.Name} {GetComparison(searchFilterModel)} '{searchFilterModel.Value}'";
		}

		private string GetComparison(SearchFilterModel searchFilterModel)
		{
			switch (searchFilterModel.Comprison)
			{
				case Comparisons.Equal:
					return Constants.ODataEqual;

				case Comparisons.NotEqual:
					return Constants.ODataNotEqual;

				default:
					throw new NotImplementedException($"Comparison {searchFilterModel.Comprison} is not implemented or is invalid.");
			}
		}
	}
}
