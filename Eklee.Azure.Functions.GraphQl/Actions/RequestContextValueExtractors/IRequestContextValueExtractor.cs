using FastMember;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Actions.RequestContextValueExtractors
{
	public interface IRequestContextValueExtractor
	{
		Task<object> GetValueAsync(IGraphRequestContext graphRequestContext, Member member);
	}
}
