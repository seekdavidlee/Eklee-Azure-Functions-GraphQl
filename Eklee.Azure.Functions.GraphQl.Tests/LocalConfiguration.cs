using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public static class LocalConfiguration
	{
		public static IConfiguration Get()
		{
			string currentDir = Directory.GetCurrentDirectory();
			$"LocalConfiguration: {currentDir}".Log();

			var builder = new ConfigurationBuilder();
			builder.SetBasePath(currentDir);
			builder.AddJsonFile("local.settings.json");
			return builder.Build();
		}
	}
}