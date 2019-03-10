using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Example.TestInMemory
{
	public class TestInMemoryMutation : ObjectGraphType
	{
		public TestInMemoryMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutation";

			inputBuilderFactory.Create<Model1>(this)
				.ConfigureInMemory<Model1>()
				.BuildInMemory()
				.DeleteAll(() => new Status { Message = "All Model1 have been removed." })
				.Build();

			inputBuilderFactory.Create<Model2>(this)
				.ConfigureInMemory<Model2>()
				.BuildInMemory()
				.DeleteAll(() => new Status { Message = "All Model2 have been removed." })
				.Build();

			inputBuilderFactory.Create<Model3>(this)
				.ConfigureInMemory<Model3>()
				.BuildInMemory()
				.DeleteAll(() => new Status { Message = "All Model3 have been removed." })
				.Build();
		}
	}
}
