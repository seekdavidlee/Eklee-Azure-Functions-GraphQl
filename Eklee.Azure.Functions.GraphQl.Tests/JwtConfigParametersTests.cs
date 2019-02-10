using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class JwtConfigParametersTests
	{
		[Fact]
		public void AudienceIsSet()
		{
			var config = Substitute.For<IConfiguration>();

			config["Security:Audience"].Returns("abc");

			var jwtConfig = new JwtConfigParameters(config);

			jwtConfig.Audience.ShouldBe("abc");
		}

		[Fact]
		public void NoIssuerIsSet()
		{
			var config = Substitute.For<IConfiguration>();

			var jwtConfig = new JwtConfigParameters(config);

			jwtConfig.Issuers.Length.ShouldBe(0);
		}

		[Fact]
		public void SingleIssuerIsSet()
		{
			var config = Substitute.For<IConfiguration>();

			config["Security:Issuers"].Returns("abc");

			var jwtConfig = new JwtConfigParameters(config);

			jwtConfig.Issuers.Length.ShouldBe(1);
			jwtConfig.Issuers[0].ShouldBe("abc");
		}

		[Fact]
		public void MultipleIssuersAreSet()
		{
			var config = Substitute.For<IConfiguration>();

			config["Security:Issuers"].Returns("abc def ghi");

			var jwtConfig = new JwtConfigParameters(config);

			jwtConfig.Issuers.Length.ShouldBe(3);
			jwtConfig.Issuers[0].ShouldBe("abc");
			jwtConfig.Issuers[1].ShouldBe("def");
			jwtConfig.Issuers[2].ShouldBe("ghi");
		}
	}
}
