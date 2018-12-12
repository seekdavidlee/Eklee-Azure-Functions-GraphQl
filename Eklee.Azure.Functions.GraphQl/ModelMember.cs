using System;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelMember
	{
		private readonly TypeAccessor _typeAccessor;

		public ModelMember(Type sourceType, TypeAccessor typeAccessor, /*string path,*/ Member member, bool isOptional)
		{
			_typeAccessor = typeAccessor;
			//Path = path;
			Member = member;
			IsOptional = isOptional;
			SourceType = sourceType;
			//if (IsNested) SetNestedMember();
		}

		//public string Path { get; }
		public bool IsOptional { get; }
		public Member Member { get; }

		//private void SetNestedMember()
		//{
		//	var levels = Path.Count(x => x == '.');
		//	var paths = Path.Split('.');

		//	for (var level = 0; level < levels; level++)
		//	{
		//		_typeAccessor = TypeAccessor.Create(Member.Type);
		//		SourceType = Member.Type;
		//		Member = _typeAccessor.GetMembers().Single(x => x.Name == paths[level + 1]);
		//	}
		//}

		//private bool IsNested => Path.Count(x => x == '.') > 0;

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
