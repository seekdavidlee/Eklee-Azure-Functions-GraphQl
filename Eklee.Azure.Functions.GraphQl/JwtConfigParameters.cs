using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl
{
	public class JwtConfigParameters : IJwtTokenValidatorParameters
	{
		public JwtConfigParameters(IConfiguration configuration)
		{
			Audience = configuration["Security:Audience"];

			var issuers = configuration["Security:Issuers"];

			if (!string.IsNullOrEmpty(issuers))
			{
				Issuers = issuers.Split(' ');
			}
			else
			{
				Issuers = new string[] { };
			}
		}

		public string Audience { get; }
		public string[] Issuers { get; }
	}
}
