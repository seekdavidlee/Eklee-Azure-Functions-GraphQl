using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Repository.TableStorage;
using FastMember;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.TableStorage
{
	public class ComparisonStringDto
	{
		public string Value { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class TableStorageComparisonStringTests
	{
		private readonly TableStorageComparisonString _tableStorageComparisonString = new TableStorageComparisonString();

		[Fact]
		public void CanHandleString()
		{
			_tableStorageComparisonString.CanHandle(new QueryParameter { ContextValue = new ContextValue { Values = new List<object> { "abc" } } }).ShouldBe(true);
		}

		[Fact]
		public void CanGenerateForStringEquals()
		{
			var type = typeof(ComparisonStringDto);
			var accessor = TypeAccessor.Create(type);
			var members = accessor.GetMembers();

			_tableStorageComparisonString.CanHandle(
				new QueryParameter { ContextValue = new ContextValue { Values = new List<object> { "abc" }, Comparison = Comparisons.Equal }, MemberModel = new ModelMember(type, accessor, members.Single(), false) });
			string.IsNullOrEmpty(_tableStorageComparisonString.Generate()).ShouldBe(false);
		}

		[Fact]
		public void CannotGenerateForStringNoneEquals()
		{
			var type = typeof(ComparisonStringDto);
			var accessor = TypeAccessor.Create(type);
			var members = accessor.GetMembers();

			_tableStorageComparisonString.CanHandle(
				new QueryParameter { ContextValue = new ContextValue { Values = new List<object> { "abc" } }, MemberModel = new ModelMember(type, accessor, members.Single(), false) });
			string.IsNullOrEmpty(_tableStorageComparisonString.Generate()).ShouldBe(true);
		}

		[Fact]
		public void CannotHandleInt()
		{
			_tableStorageComparisonString.CanHandle(new QueryParameter { ContextValue = new ContextValue { Values = new List<object> { 5 } } }).ShouldBe(false);
		}

		[Fact]
		public void CannotHandleDate()
		{
			_tableStorageComparisonString.CanHandle(new QueryParameter { ContextValue = new ContextValue { Values = new List<object> { DateTime.Today } } }).ShouldBe(false);
		}
	}
}
