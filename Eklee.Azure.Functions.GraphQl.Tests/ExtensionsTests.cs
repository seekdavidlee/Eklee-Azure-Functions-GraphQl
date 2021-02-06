using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Attributes;
using FastMember;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public class DtoWithoutKeyAttribute
	{
		public int Id { get; set; }
	}

	public class DtoWithKeyAndPartition
	{
		[Key]
		public string SomeKeyOne { get; set; }

		[PartitionKey]
		public string SomePartOne { get; set; }

		public string Value { get; set; }
	}

	public class DtoWithOptional
	{
		[ModelField(false, true)]
		public DateTime? Value { get; set; }
	}

	public class DtoWithOptional2
	{
		[ModelField(false, false)]
		public DateTime? Value { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class ExtensionsTests
	{
		[Fact]
		public void ThrowExceptionIfMissingKeyAttribute()
		{
			Should.Throw<InvalidOperationException>(() => new DtoWithoutKeyAttribute().GetKey());
		}

		[Fact]
		public void CanGetModelKey()
		{
			var model1 = new DtoWithKeyAndPartition
			{
				SomeKeyOne = "One",
				SomePartOne = "Part1"
			};

			var ta = TypeAccessor.Create(typeof(DtoWithKeyAndPartition));
			var key = ta.GetModelKey(model1);
			key.ShouldContain("One");
			key.ShouldContain("Part1");
		}

		[Fact]
		public void UseNullWhenOptionalIsTrueAndAttributeFlagIsSet()
		{
			var ta = TypeAccessor.Create(typeof(DtoWithOptional));

			var modelMember = new ModelMember(null, null, ta.GetMembers().Single(), true);
			modelMember.UseNullWhenOptional().ShouldBeTrue();
		}

		[Fact]
		public void DoNotUseNullWhenOptionalIsFalseAndAttributeFlagIsSet()
		{
			var ta = TypeAccessor.Create(typeof(DtoWithOptional));

			var modelMember = new ModelMember(null, null, ta.GetMembers().Single(), false);
			modelMember.UseNullWhenOptional().ShouldBeFalse();
		}

		[Fact]
		public void DoNotUseNullWhenOptionalIsTrueButAttributeFlagIsNotSet()
		{
			var ta = TypeAccessor.Create(typeof(DtoWithOptional2));

			var modelMember = new ModelMember(null, null, ta.GetMembers().Single(), true);
			modelMember.UseNullWhenOptional().ShouldBeFalse();
		}
	}
}
