using Eklee.Azure.Functions.GraphQl.Example.Models;
using Eklee.Azure.Functions.GraphQl.Example.TestSearch2.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2
{
	public class TestSearchMutation2 : ObjectGraphType
	{
		public TestSearchMutation2(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutation";

			var api = configuration["Search:ApiKey"];
			var serviceName = configuration["Search:ServiceName"];

			inputBuilderFactory.Create<MySearch4>(this)
				.DeleteAll(() => new Status { Message = "All MySearch searches have been deleted." })
				.ConfigureSearch<MySearch4>()
				.AddApiKey(api)
				.AddServiceName(serviceName)
				.AddPrefix("lcl1")
				.BuildSearch()
				.Build();
		}
	}
}
