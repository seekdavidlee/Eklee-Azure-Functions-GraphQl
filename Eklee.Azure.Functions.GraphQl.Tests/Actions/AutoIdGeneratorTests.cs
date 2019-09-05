using Eklee.Azure.Functions.GraphQl.Actions;
using Eklee.Azure.Functions.GraphQl.Actions.AutoIdPatterns;
using Eklee.Azure.Functions.GraphQl.Attributes;
using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Actions
{

	public class AutoIdGenMock
	{
		[AutoId(Attributes.AutoIdPatterns.Guid)]
		public string Id { get; set; }

		public string Value { get; set; }
	}

	public class CustomAutoIdGenMock
	{
		[AutoId(Attributes.AutoIdPatterns.Custom, typeof(AutoIdCustom))]
		public int Id { get; set; }

		public string Value { get; set; }
	}

	public class AutoIdCustom : IAutoIdPattern
	{
		private static int _counter = 0;
		public object Generate(object item, Member member)
		{
			_counter += 1;
			return _counter;
		}
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class AutoIdGeneratorTests
	{
		private readonly AutoIdGenerator _autoIdGenerator = new AutoIdGenerator(new List<IAutoIdPattern>
		{
			new GuidAutoIdPattern(),
			new AutoIdCustom()
		});

		[Theory]
		[InlineData(MutationActions.BatchCreate)]
		[InlineData(MutationActions.BatchCreateOrUpdate)]
		public async Task BatchCreateOrCreateOrUpdateCanSetItemsWithGuid(MutationActions mutationAction)
		{
			var mutation = new MutationActionItem<AutoIdGenMock>();
			mutation.Action = mutationAction;
			mutation.Items = new List<AutoIdGenMock>
			{
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "a"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "b"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "c"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "d"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "e"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "f"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "g"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "h"}
			};

			await _autoIdGenerator.TryHandlePreItem(mutation);

			foreach (var item in mutation.Items)
			{
				item.Id.ShouldNotBeNullOrEmpty();
				item.Id.ShouldNotBe(AutoIdGenerator.Marker);
				item.Id.Length.ShouldBeGreaterThan(1);
			}
		}

		[Theory]
		[InlineData(MutationActions.Create)]
		[InlineData(MutationActions.CreateOrUpdate)]
		public async Task SingleCreateOrCreateOrUpdateCanSetItemWithGuid(MutationActions mutationAction)
		{
			var mutation = new MutationActionItem<AutoIdGenMock>();
			mutation.Action = mutationAction;
			mutation.Item = new AutoIdGenMock { Id = AutoIdGenerator.Marker, Value = "a" };

			await _autoIdGenerator.TryHandlePreItem(mutation);

			mutation.Item.Id.ShouldNotBeNullOrEmpty();
			mutation.Item.Id.ShouldNotBe(AutoIdGenerator.Marker);
			mutation.Item.Id.Length.ShouldBeGreaterThan(1);
		}

		[Theory]
		[InlineData(MutationActions.Create)]
		[InlineData(MutationActions.CreateOrUpdate)]
		public async Task SingleCreateOrCreateOrUpdateShouldNotSetItemWithGuidWhenItHasValue(MutationActions mutationAction)
		{
			var mutation = new MutationActionItem<AutoIdGenMock>();
			mutation.Action = mutationAction;
			mutation.Item = new AutoIdGenMock { Id = "Foo2", Value = "a" };

			await _autoIdGenerator.TryHandlePreItem(mutation);

			mutation.Item.Id.ShouldBe("Foo2");
		}

		[Theory]
		[InlineData(MutationActions.BatchCreate)]
		[InlineData(MutationActions.BatchCreateOrUpdate)]
		public async Task BatchCreateOrCreateOrUpdateCanSetObjectItemsWithGuid(MutationActions mutationAction)
		{
			var mutation = new MutationActionItem<AutoIdGenMock>();
			mutation.Action = mutationAction;
			mutation.ObjectItems = new List<object>
			{
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "a"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "b"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "c"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "d"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "e"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "f"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "g"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "h"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "a"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "b"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "c"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "d"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "e"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "f"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "g"},
				new AutoIdGenMock{Id=AutoIdGenerator.Marker, Value = "h"}
			};

			await _autoIdGenerator.TryHandlePreItem(mutation);

			foreach (AutoIdGenMock item in mutation.ObjectItems)
			{
				item.Id.ShouldNotBeNullOrEmpty();
				item.Id.ShouldNotBe(AutoIdGenerator.Marker);
				item.Id.Length.ShouldBeGreaterThan(1);
			}
		}

		[Theory]
		[InlineData(MutationActions.Create)]
		[InlineData(MutationActions.CreateOrUpdate)]
		public async Task SingleCreateOrCreateOrUpdateCanSetObjectItemWithGuid(MutationActions mutationAction)
		{
			var mutation = new MutationActionItem<AutoIdGenMock>();
			mutation.Action = mutationAction;
			mutation.ObjectItem = new AutoIdGenMock { Id = AutoIdGenerator.Marker, Value = "a" };

			await _autoIdGenerator.TryHandlePreItem(mutation);

			((AutoIdGenMock)mutation.ObjectItem).Id.ShouldNotBeNullOrEmpty();
			((AutoIdGenMock)mutation.ObjectItem).Id.ShouldNotBe(AutoIdGenerator.Marker);
			((AutoIdGenMock)mutation.ObjectItem).Id.Length.ShouldBeGreaterThan(1);
		}

		[Theory]
		[InlineData(MutationActions.Create)]
		[InlineData(MutationActions.CreateOrUpdate)]
		public async Task SingleCreateOrCreateOrUpdateShouldNotSetObjectItemWithGuidWhenItHasValue(MutationActions mutationAction)
		{
			var mutation = new MutationActionItem<AutoIdGenMock>();
			mutation.Action = mutationAction;
			mutation.ObjectItem = new AutoIdGenMock { Id = "foo1", Value = "a" };

			await _autoIdGenerator.TryHandlePreItem(mutation);

			((AutoIdGenMock)mutation.ObjectItem).Id.ShouldBe("foo1");
		}
	}
}
