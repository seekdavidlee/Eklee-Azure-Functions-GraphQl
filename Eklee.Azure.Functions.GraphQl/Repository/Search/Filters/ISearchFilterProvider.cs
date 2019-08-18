using FastMember;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search.Filters
{
	public interface ISearchFilterProvider
	{
		string GenerateStringFilter(IEnumerable<QueryParameter> queryParameters, MemberSet members);
	}
}
