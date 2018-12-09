using System.Linq;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelMember
	{
		private TypeAccessor _typeAccessor;

		public ModelMember(TypeAccessor typeAccessor)
		{
			_typeAccessor = typeAccessor;
		}

		public string Path { get; set; }
		public bool IsOptional { get; set; }
		public Member Member { private get; set; }
		public Member PathMember => IsNested() ? GetNestedMember() : Member;

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

		private bool IsNested()
		{
			return Path.Count(x => x == '.') > 0;
		}

		public string Name => (IsNested() ? GetNestedMember() : Member).Name.ToLower();

		public string Description => (IsNested() ? GetNestedMember() : Member).GetDescription();

		public bool PathMemberValueEquals(object targetObject, object compareValue)
		{
			return _typeAccessor[targetObject, PathMember.Name].Equals(compareValue);
		}
	}
}
