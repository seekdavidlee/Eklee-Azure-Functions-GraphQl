using System;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using Eklee.Azure.Functions.GraphQl.Validations;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Example.Validations
{
	public class MyValidation : IModelValidation
	{
		public bool CanHandle(Type type)
		{
			return type == typeof(Model4);
		}

		public bool TryAssertMemberValueIsValid(Member member, object value, out string errorCode, out string message)
		{
			if (member.Name == "DateField")
			{
				DateTime result;
				if (DateTime.TryParse(value.ToString(), out result))
				{
					if (result == DateTime.MinValue)
					{
						errorCode = "DateTimeError";
						message = "DateTime cannot be Min Value.";
						return false;
					}
				}
				else
				{
					errorCode = "DateTimeError";
					message = "DateTime is invalid.";
					return false;
				}
			}

			errorCode = null;
			message = null;
			return true;
		}
	}
}
