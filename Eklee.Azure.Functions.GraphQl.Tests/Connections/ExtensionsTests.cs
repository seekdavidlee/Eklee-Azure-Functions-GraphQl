using Eklee.Azure.Functions.GraphQl.Connections;
using FastMember;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Connections
{
	public class ConnectionsFooChild
	{
		public string Id { get; set; }
		public int Value { get; set; }
	}

	public class ConnectionsFoo
	{
		public string Name { get; set; }

		public List<ConnectionsFooChild> ConnectionsFooChildren { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class ExtensionsTests
	{
		private TypeAccessor _typeAccessor;

		public ExtensionsTests()
		{
			_typeAccessor = TypeAccessor.Create(typeof(ConnectionsFoo));
		}

		[Fact]
		public void IsList()
		{
			var listMember = _typeAccessor.GetMembers().Single(x => x.Name == "ConnectionsFooChildren");
			listMember.IsList().ShouldBeTrue();
		}

		[Fact]
		public void IsNotList()
		{
			var listMember = _typeAccessor.GetMembers().Single(x => x.Name == "Name");
			listMember.IsList().ShouldBeFalse();
		}


		[Fact]
		public void ShouldCreateNewListAndAddItems()
		{
			var parent = new ConnectionsFoo();
			parent.Name = "parent1";

			var child1 = new ConnectionsFooChild();
			child1.Id = "child1";

			var listMember = _typeAccessor.GetMembers().Single(x => x.Name == "ConnectionsFooChildren");

			parent.ConnectionsFooChildren.ShouldBeNull();

			listMember.CreateNewListIfNullThenAddItemToList(_typeAccessor, parent, child1);

			parent.ConnectionsFooChildren.ShouldNotBeNull();

			var child2 = new ConnectionsFooChild();
			child2.Id = "child2";

			listMember.CreateNewListIfNullThenAddItemToList(_typeAccessor, parent, child2);

			parent.ConnectionsFooChildren.Count.ShouldBe(2);			

			parent.ConnectionsFooChildren[0].Id.ShouldBe("child1");
			parent.ConnectionsFooChildren[1].Id.ShouldBe("child2");
		}
	}
}
