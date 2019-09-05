using Eklee.Azure.Functions.GraphQl.Actions.AutoIdPatterns;
using FastMember;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Actions.AutoIdPatterns
{
	public class AutoIdMock
	{
		public string ValStr { get; set; }
		public int ValInt { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class GuidAutoIdPatternTests
	{
		private readonly GuidAutoIdPattern _guidAutoIdPattern = new GuidAutoIdPattern();
		private readonly TypeAccessor _typeAccessor = TypeAccessor.Create(typeof(AutoIdMock));

		[Fact]
		public void CanGenerateStringWhenMemberIsString()
		{
			var member = _typeAccessor.GetMembers().Single(x => x.Name == "ValStr");
			var o = _guidAutoIdPattern.Generate(new AutoIdMock { }, member);
			o.ShouldBeOfType<string>();
			((string)o).ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void ShouldThrowExceptionWhenMemberIsNotString()
		{
			var member = _typeAccessor.GetMembers().Single(x => x.Name == "ValInt");
			Should.Throw<InvalidOperationException>(() => _guidAutoIdPattern.Generate(new AutoIdMock { }, member));
		}
	}
}
