using System;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelMember
	{
		private readonly TypeAccessor _typeAccessor;

		public ModelMember(Type sourceType, TypeAccessor typeAccessor, Member member, bool isOptional)
		{
			_typeAccessor = typeAccessor;
			Member = member;
			IsOptional = isOptional;
			SourceType = sourceType;
		}

		public bool IsOptional { get; }
		public Member Member { get; }

		public string Name => Member.Name.ToLower();

		public string Description => Member.GetDescription();

		public bool IsString => Member.Type == typeof(string);

		public bool PathMemberValueEquals(object targetObject, object compareValue)
		{
			return _typeAccessor[targetObject, Member.Name].Equals(compareValue);
		}

		public Type SourceType { get; }
	}
}
