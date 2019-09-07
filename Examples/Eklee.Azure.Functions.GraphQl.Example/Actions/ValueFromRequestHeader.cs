using Eklee.Azure.Functions.GraphQl.Actions.RequestContextValueExtractors;
using FastMember;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Example.Actions
{
	public class ValueFromRequestHeader : IRequestContextValueExtractor
	{
		public Task<object> GetValueAsync(IGraphRequestContext graphRequestContext, Member member)
		{
			var headerVal = graphRequestContext.HttpRequest.Request.Headers["SomeThingFromHeader"];
			return Task.FromResult((object)headerVal.First());
		}
	}
}
