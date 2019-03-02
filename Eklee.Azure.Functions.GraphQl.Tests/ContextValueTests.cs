using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class ContextValueTests
	{
		private readonly ContextValue _contextValue = new ContextValue();

		[Fact]
		public void SingleValueNotSetToBeFalse()
		{
			_contextValue.Values = new List<object>();
			_contextValue.IsSingleValue().ShouldBeFalse();
		}

		[Fact]
		public void SingleValueSetToBeTrue()
		{
			_contextValue.Values = new List<object> { "a" };
			_contextValue.IsSingleValue().ShouldBeTrue();
		}

		[Fact]
		public void AbleToGetFirstValue()
		{
			_contextValue.Values = new List<object> { "a" };
			_contextValue.GetFirstValue().ShouldBe("a");
		}
	}
}
