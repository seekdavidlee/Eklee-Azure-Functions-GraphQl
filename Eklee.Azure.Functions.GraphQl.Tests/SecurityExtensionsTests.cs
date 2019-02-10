using System.Collections.Generic;
using System.Security.Claims;
using Eklee.Azure.Functions.Http;
using Eklee.Azure.Functions.Http.Models;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class SecurityExtensionsTests
	{
		[Fact]
		public void ShouldGetTenantIdFromValidIssuer()
		{
			const string issuer = "https://sts.windows.net/ce2196d7-bd3d-4cbd-9596-d604e69f58c6";
			var tenantId = issuer.GetTenantIdFromIssuer();
			tenantId.ShouldBe("ce2196d7bd3d4cbd9596d604e69f58c6");
		}

		[Fact]
		public void ClaimsPrincipalContainsIssuer()
		{
			const string issuer = "https://sts.windows.net/ce2196d7-bd3d-4cbd-9596-d604e69f58c6";

			var graphContext = Substitute.For<IGraphRequestContext>();
			graphContext.HttpRequest = Substitute.For<IHttpRequestContext>();
			graphContext.HttpRequest.Security.Returns(new Security
			{
				ClaimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity>
			{
				new ClaimsIdentity(new List<Claim>
				{
					new Claim("iss",issuer)
				})
			})
			});

			graphContext.ContainsIssuer(issuer).ShouldBe(true);
		}

		[Fact]
		public void ClaimsPrincipalDoesNotContainsIssuer()
		{
			const string issuer = "https://sts.windows.net/ce2196d7-bd3d-4cbd-9596-d604e69f58c6";

			var graphContext = Substitute.For<IGraphRequestContext>();
			graphContext.HttpRequest = Substitute.For<IHttpRequestContext>();
			graphContext.HttpRequest.Security.Returns(new Security
			{
				ClaimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity>
				{
					new ClaimsIdentity(new List<Claim>
					{
						new Claim("iss","https://sts.windows.net/8505a1f5-514e-4021-aa8f-42aee7a05454")
					})
				})
			});

			graphContext.ContainsIssuer(issuer).ShouldBe(false);
		}
	}
}
