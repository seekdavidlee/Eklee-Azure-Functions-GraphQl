using System;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class SecurityExtensions
	{
		private const string AzureAdSts = "https://sts.windows.net/";

		public static string GetTenantIdFromIssuer(this string issuer)
		{
			var index = issuer.IndexOf(AzureAdSts, StringComparison.Ordinal);
			return issuer.Substring(index > -1 ? AzureAdSts.Length : 0).TrimEnd('/').Replace("-", "");
		}

		public static bool ContainsIssuer(this IGraphRequestContext graphRequestContext, string issuer)
		{
			return graphRequestContext.HttpRequest != null &&
				   graphRequestContext.HttpRequest.Security.ClaimsPrincipal != null &&
				   graphRequestContext.HttpRequest.Security.ClaimsPrincipal.Claims.Any(
					   x => x.Type == "iss" && x.Value == issuer);
		}
	}
}
