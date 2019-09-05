using FastMember;
using System;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Actions.RequestContextValueExtractors
{
	public class TrustFrameworkPolicyRequestContextValueExtractor : IRequestContextValueExtractor
	{
		public Task<object> GetValue(IGraphRequestContext graphRequestContext, Member member)
		{
			if (graphRequestContext.HttpRequest.Security != null)
			{
				var tfpClaim = graphRequestContext.HttpRequest.Security.ClaimsPrincipal.FindFirst(x => x.Type == "tfp");
				if (tfpClaim != null)
				{
					return Task.FromResult((object) tfpClaim.Value);
				}
			}

			throw new InvalidOperationException("Unable to get Trust Framework Policy (tfp) claim.");
		}
	}
}
