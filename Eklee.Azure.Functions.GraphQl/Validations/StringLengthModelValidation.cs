using System;
using System.ComponentModel.DataAnnotations;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Validations
{
	public class StringLengthModelValidation : IModelValidation
	{
		public bool CanHandle(Type type)
		{
			// Handle all types.
			return true;
		}

		public bool TryAssertMemberValueIsValid(Member member, object value, out string errorCode, out string message)
		{
			StringLengthAttribute stringLength = member.GetAttribute(typeof(StringLengthAttribute), false) as StringLengthAttribute;

			if (stringLength != null && !IsValid(stringLength, value, out message))
			{
				errorCode = "InvalidStringLength";
				return false;
			}
			errorCode = null;
			message = null;

			return true;
		}

		private bool IsValid(StringLengthAttribute stringLength, object value, out string message)
		{
			if (value is string valueString)
			{
				if (string.IsNullOrEmpty(valueString) && (stringLength.MinimumLength > 0 || stringLength.MaximumLength > 0))
				{
					message = $"String value must be more than {stringLength.MinimumLength} in length.";
					return false;
				}

				if (valueString.Length < stringLength.MinimumLength)
				{
					message = $"String value must be more than {stringLength.MinimumLength} in length.";
					return false;
				}

				if (valueString.Length > stringLength.MaximumLength)
				{
					message = $"String value must be less than {stringLength.MaximumLength} in length.";
					return false;
				}
			}

			message = "";
			return true;
		}
	}
}
