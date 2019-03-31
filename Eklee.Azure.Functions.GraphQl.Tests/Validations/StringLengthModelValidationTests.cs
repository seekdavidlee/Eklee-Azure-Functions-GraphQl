using Eklee.Azure.Functions.GraphQl.Validations;
using FastMember;
using Shouldly;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Validations
{
	public class StringLengthTestModel1
	{
		[StringLength(10, MinimumLength = 5)]
		public string MyField { get; set; }

		[StringLength(10)]
		public string MyField2 { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class StringLengthModelValidationTests
	{
		private readonly StringLengthModelValidation _stringLengthModelValidation;
		public StringLengthModelValidationTests()
		{
			_stringLengthModelValidation = new StringLengthModelValidation();
		}

		[Fact]
		public void CanHandleModel()
		{
			_stringLengthModelValidation.CanHandle(typeof(StringLengthTestModel1)).ShouldBeTrue();
		}

		[Fact]
		public void ReturnFalseIfMaxLengthIsExceeded()
		{
			var member = GetMember<StringLengthTestModel1, string>(x => x.MyField);

			string errorCode;
			string message;

			_stringLengthModelValidation.TryAssertMemberValueIsValid(member,
				"abcdefghijk", out errorCode, out message).ShouldBeFalse();

			errorCode.ShouldNotBeNullOrEmpty();
			message.ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void ReturnFalseIfMinLengthNotMet()
		{
			var member = GetMember<StringLengthTestModel1, string>(x => x.MyField);

			string errorCode;
			string message;

			_stringLengthModelValidation.TryAssertMemberValueIsValid(member,
				"abcd", out errorCode, out message).ShouldBeFalse();

			errorCode.ShouldNotBeNullOrEmpty();
			message.ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void ReturnTrueIfInRange()
		{
			var member = GetMember<StringLengthTestModel1, string>(x => x.MyField);

			string errorCode;
			string message;

			_stringLengthModelValidation.TryAssertMemberValueIsValid(member,
				"abcdefghij", out errorCode, out message).ShouldBeTrue();

			errorCode.ShouldBeNullOrEmpty();
			message.ShouldBeNullOrEmpty();
		}

		[Fact]
		public void ReturnTrueIfSetForEmptyStringNotForMax()
		{
			var member = GetMember<StringLengthTestModel1, string>(x => x.MyField2);

			string errorCode;
			string message;

			_stringLengthModelValidation.TryAssertMemberValueIsValid(member,
				"", out errorCode, out message).ShouldBeTrue();

			errorCode.ShouldBeNullOrEmpty();
			message.ShouldBeNullOrEmpty();
		}

		private Member GetMember<T, TProperty>(Expression<Func<T, TProperty>> expression)
		{
			MemberExpression memberExpression = expression.Body as MemberExpression ?? (expression.Body as UnaryExpression)?.Operand as MemberExpression;

			if (memberExpression != null)
			{
				return TypeAccessor.Create(typeof(T)).GetMembers().Single(x =>
				   x.Name.ToLower() == memberExpression.Member.Name.ToLower());
			}

			throw new InvalidOperationException("Unable to get object property.");
		}
	}
}
