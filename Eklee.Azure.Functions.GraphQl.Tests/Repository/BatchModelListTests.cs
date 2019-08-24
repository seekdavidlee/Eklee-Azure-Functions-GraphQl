using Eklee.Azure.Functions.GraphQl.Repository;
using Shouldly;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository
{
	public class BatchItem1
	{
		[Key]
		public string Id { get; set; }

		public string Value { get; set; }
	}

	public class BatchItem2
	{
		[Key]
		public string Id1 { get; set; }

		[Key]
		public string Id2 { get; set; }

		public string Value { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class BatchModelListTests
	{
		private readonly BatchModelList _list1 = new BatchModelList(typeof(BatchItem1));
		private readonly BatchModelList _list2 = new BatchModelList(typeof(BatchItem2));

		[Fact]
		public void CannotAddDuplicateOnEntityWithSingleKey()
		{
			_list1.Add(new BatchItem1 { Id = "1", Value = "aaa" });
			_list1.Add(new BatchItem1 { Id = "1", Value = "bbb" });
			_list1.Add(new BatchItem1 { Id = "2", Value = "ccc" });

			_list1.Items.Count.ShouldBe(2);
			_list1.Items.Select(x => (BatchItem1)x).Single(x => x.Id == "1" &&
			  x.Value == "aaa").ShouldNotBeNull();
			_list1.Items.Select(x => (BatchItem1)x).Single(x => x.Id == "2" &&
			  x.Value == "ccc").ShouldNotBeNull();
		}

		[Fact]
		public void CannotAddDuplicateOnEntityWithTwoKeys()
		{
			_list2.Add(new BatchItem2 { Id1 = "1", Id2 = "a", Value = "aaa" });
			_list2.Add(new BatchItem2 { Id1 = "1", Id2 = "a", Value = "bbb" });
			_list2.Add(new BatchItem2 { Id1 = "1", Id2 = "b", Value = "ccc" });
			_list2.Add(new BatchItem2 { Id1 = "2", Id2 = "a", Value = "ddd" });

			_list2.Items.Count.ShouldBe(3);
			_list2.Items.Select(x => (BatchItem2)x).Single(x => x.Id1 == "1" &&
				x.Id2 == "a" && x.Value == "aaa").ShouldNotBeNull();
			_list2.Items.Select(x => (BatchItem2)x).Single(x => x.Id1 == "1" &&
				x.Id2 == "b" && x.Value == "ccc").ShouldNotBeNull();
			_list2.Items.Select(x => (BatchItem2)x).Single(x => x.Id1 == "2" &&
				x.Id2 == "a" && x.Value == "ddd").ShouldNotBeNull();
		}
	}
}
