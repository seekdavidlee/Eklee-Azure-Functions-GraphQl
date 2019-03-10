using System.ComponentModel;
using System.Linq;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	[Description("Types of Model1s")]
	public enum TestEnumTypes
	{
		[Description("Type 1")]
		TestType1,

		[Description("Type 2")]
		TestType2,

		[Description("Type 3")]
		TestType3
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class ModelEnumConventionTypeTests
	{
		private readonly ModelEnumConventionType<TestEnumTypes> _enum =
			new ModelEnumConventionType<TestEnumTypes>();


		[Fact]
		public void ShouldHaveName()
		{
			_enum.Name.ShouldBe("testenumtypes");
		}

		[Fact]
		public void ShouldHaveDescription()
		{
			_enum.Description.ShouldBe("Types of Model1s");
		}

		[Fact]
		public void ShouldHaveThreeEnumValues()
		{
			_enum.Values.Count().ShouldBe(3);
		}

		[Fact]
		public void ShouldContainTestType1()
		{
			_enum.Values.Any(x => x.Name == "testtype1").ShouldBeTrue();
		}

		[Fact]
		public void ShouldContainTestType2()
		{
			_enum.Values.Any(x => x.Name == "testtype2").ShouldBeTrue();
		}

		[Fact]
		public void ShouldContainTestType3()
		{
			_enum.Values.Any(x => x.Name == "testtype3").ShouldBeTrue();
		}

		[Fact]
		public void ShouldContainTestType1Description()
		{
			_enum.Values.Any(x => x.Description == "Type 1").ShouldBeTrue();
		}

		[Fact]
		public void ShouldContainTestType2Description()
		{
			_enum.Values.Any(x => x.Description == "Type 2").ShouldBeTrue();
		}

		[Fact]
		public void ShouldContainTestType3Description()
		{
			_enum.Values.Any(x => x.Description == "Type 3").ShouldBeTrue();
		}
	}
}
