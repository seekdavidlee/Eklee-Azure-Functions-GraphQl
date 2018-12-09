using System.Linq;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelMember
	{
		private TypeAccessor _typeAccessor;

		public ModelMember(TypeAccessor typeAccessor, string path, Member member, bool isOptional)
		{
			_typeAccessor = typeAccessor;
			Path = path;
			Member = member;
			IsOptional = isOptional;
			if (IsNested) Member = GetNestedMember();
		}

		public string Path { get; }
		public bool IsOptional { get; }
		public Member Member { get; }

		private Member _nestedMember;
		private Member GetNestedMember()
		{
			if (_nestedMember != null) return _nestedMember;

			var levels = Path.Count(x => x == '.');
			var paths = Path.Split('.');

			for (var level = 0; level < levels; level++)
			{
				_typeAccessor = TypeAccessor.Create(Member.Type);
				_nestedMember = _typeAccessor.GetMembers().Single(x => x.Name == paths[level + 1]);
			}

			return _nestedMember;
		}

		public bool IsNested => Path.Count(x => x == '.') > 0;

		public string Name => Member.Name.ToLower();

		public string Description => Member.GetDescription();

		public bool IsString => Member.Type == typeof(string);

		public bool PathMemberValueEquals(object targetObject, object compareValue)
		{
			return _typeAccessor[targetObject, Member.Name].Equals(compareValue);
		}
	}
}
