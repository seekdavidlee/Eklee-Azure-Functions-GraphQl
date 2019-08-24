using System.Collections.Generic;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using NSubstitute;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository
{
	public class BatchAddItem
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class GraphQlRepositoryExtensionsTests
	{
		[Fact]
		public async Task CanBatchAdd()
		{
			List<object> list = new List<object>
			{
				new BatchAddItem {Id = "1", Name = "Foo 1"},
				new BatchAddItem {Id = "2", Name = "Foo 2"},
				new BatchAddItem {Id = "3", Name = "Foo 3"}
			};

			var mock = Substitute.For<IGraphQlRepository>();
			await mock.BatchAddAsync(typeof(BatchAddItem), list, null);

			Received.InOrder(async () =>
			{
				await mock.BatchAddAsync(Arg.Is<List<BatchAddItem>>(x => x.Count == 3), null);
			});
		}
	}
}
