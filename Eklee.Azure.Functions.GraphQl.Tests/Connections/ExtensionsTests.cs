using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Tests.Models;
using Newtonsoft.Json;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Connections
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class ExtensionsTests
	{
		[Fact]
		public void CanPopulate()
		{
			var friend = new Model1Friend
			{
				Id = "bf1",
				Field1 = "dss",
				Field2 = 544
			};

			var list = new List<ConnectionEdge> {
				new ConnectionEdge
				{
					SourceFieldName = "BestFriend",
					SourceId = "f1",
					MetaType = friend.GetType().AssemblyQualifiedName,
					MetaValue = JsonConvert.SerializeObject(friend)
				}
			};

			var m1 = new Model1
			{
				Id = "f1",
				Field1 = "so",
				Field2 = 12
			};
			var models = new List<object> { m1 };

			list.Populate(models);

			m1.BestFriend.ShouldNotBeNull();
			m1.BestFriend.Id.ShouldBe(friend.Id);
			m1.BestFriend.Field1.ShouldBe(friend.Field1);
			m1.BestFriend.Field2.ShouldBe(friend.Field2);
		}
	}
}
