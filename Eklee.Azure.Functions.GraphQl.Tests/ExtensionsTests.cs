using System;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public class DtoWithoutKeyAttribute
	{
		public int Id { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class ExtensionsTests
	{
		[Fact]
		public void ThrowExceptionIfMissingKeyAttribute()
		{
			Should.Throw<InvalidOperationException>(() => new DtoWithoutKeyAttribute().GetKey());
		}
	}
}
