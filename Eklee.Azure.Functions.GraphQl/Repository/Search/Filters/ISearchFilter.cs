using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search.Filters
{
	public interface ISearchFilter
	{
		bool CanHandle(Comparisons comparison, Member member);
		string GetFilter(SearchFilterModel searchFilterModel, Member member);
	}
}
