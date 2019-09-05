using FastMember;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Actions.RequestContextValueExtractors
{
	public interface IRequestContextValueExtractor
	{
		Task<object> GetValue(IGraphRequestContext graphRequestContext, Member member);
	}
}
