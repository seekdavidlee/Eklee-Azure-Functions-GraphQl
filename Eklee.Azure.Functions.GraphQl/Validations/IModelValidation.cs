using FastMember;
using System;

namespace Eklee.Azure.Functions.GraphQl.Validations
{
	public interface IModelValidation
	{
		bool CanHandle(Type type);
		bool TryAssertMemberValueIsValid(Member member, object value, out string errorCode, out string message);
	}
}
