using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Tests.Models;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Connections
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class ListConnectionEdgeQueryParameterTests
	{
		private readonly ConnectionEdgeResolver _connectionFieldResolver =
			new ConnectionEdgeResolver();

		[Fact]
		public void CanList()
		{
			var models = new List<Model1>
			{
				new Model1 { Id = "f1" },
				new Model1 { Id = "f2" },
				new Model1 { Id = "f3" },
				new Model1 { Id = "f4" }
			};

			var list = _connectionFieldResolver.ListConnectionEdgeQueryParameter(models);
			list.Count.ShouldBe(4);

			list.SingleOrDefault(item => item.SourceId == "f1").ShouldNotBeNull();
			list.SingleOrDefault(item => item.SourceId == "f2").ShouldNotBeNull();
			list.SingleOrDefault(item => item.SourceId == "f3").ShouldNotBeNull();
			list.SingleOrDefault(item => item.SourceId == "f4").ShouldNotBeNull();

			list.ForEach(item =>
			{
				item.SourceFieldName.ShouldNotBeNullOrEmpty();
				item.SourceType.ShouldNotBeNullOrEmpty();
			});
		}
	}
}
