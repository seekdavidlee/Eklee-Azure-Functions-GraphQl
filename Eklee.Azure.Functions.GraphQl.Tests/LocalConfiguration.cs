using System.IO;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public static class LocalConfiguration
	{
		public static IConfiguration Get()
		{
			var builder = new ConfigurationBuilder();
			builder.SetBasePath(Directory.GetCurrentDirectory());
			builder.AddJsonFile("local.settings.json");
			return builder.Build();
		}
	}
}