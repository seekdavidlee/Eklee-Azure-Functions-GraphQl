using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using FastMember;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.InMemory
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class InMemoryCompareStringTests
	{
		private readonly InMemoryCompareString _memoryCompareString = new InMemoryCompareString();

		private ModelMember GetModelMember(string name)
		{
			var type = typeof(InMemItem);
			var ta = TypeAccessor.Create(type);
			return new ModelMember(type, ta, ta.GetMembers().Single(x => x.Name == name), false);
		}

		private QueryParameter GetQueryParameter(string name, object value, Comparisons comparisons)
		{
			return new QueryParameter
			{
				ContextValue = new ContextValue
				{
					Comparison = comparisons,
					Values = new List<object> { value }
				},
				MemberModel = GetModelMember(name)
			};
		}

		[Fact]
		public void CannotHandleInt()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "1" };

			_memoryCompareString.CanHandle(inMem, GetQueryParameter("StrValue", 1, Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CannotHandleDate()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "1" };

			_memoryCompareString.CanHandle(inMem, GetQueryParameter("StrValue", DateTime.Now, Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CanHandleString()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "1" };

			_memoryCompareString.CanHandle(inMem, GetQueryParameter("StrValue", "foobar", Comparisons.Equal)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEqual_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "156" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "156", Comparisons.Equal)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEqual_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "321" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "156", Comparisons.Equal)).ShouldBeFalse();
		}
	}
}
