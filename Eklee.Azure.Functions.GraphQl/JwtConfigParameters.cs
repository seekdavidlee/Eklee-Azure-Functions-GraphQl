using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl
{
	public class JwtConfigParameters : IJwtTokenValidatorParameters
	{
		public JwtConfigParameters(IConfiguration configuration)
		{
			Audience = configuration["Security:Audience"];
			Issuers = configuration["Security:Issuers"].Split(' ');
		}

		public string Audience { get; }
		public string[] Issuers { get; }
	}
}
