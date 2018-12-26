using System;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelMember
	{
		public ModelMember(Type sourceType, TypeAccessor typeAccessor, Member member, bool isOptional)
		{
			TypeAccessor = typeAccessor;
			Member = member;
			IsOptional = isOptional;
			SourceType = sourceType;
		}

		public bool IsOptional { get; }

		public TypeAccessor TypeAccessor { get; }

		public Member Member { get; }

		public string Name => Member.Name.ToLower();

		public string Description => Member.GetDescription();

		public bool IsString => Member.Type == typeof(string);
		public bool IsInt => Member.Type == typeof(int);
		public bool IsDate => Member.Type == typeof(DateTime);
		public bool IsBool => Member.Type == typeof(bool);

		public Type SourceType { get; }
	}
}
