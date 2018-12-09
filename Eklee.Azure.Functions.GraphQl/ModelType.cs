using FastMember;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelType<TSourceType>
	{
		private readonly TypeAccessor _typeAccessor;
		private readonly List<Member> _members;
		public string Name { get; }

		public ModelType()
		{
			var type = typeof(TSourceType);
			_typeAccessor = TypeAccessor.Create(type);
			_members = _typeAccessor.GetMembers().ToList();
			Name = type.Name;
		}

		public void ForEach(Action<Member> memberAction)
		{
			_members.ForEach(memberAction);
		}

		public TypeAccessor GetTypeAccessor()
		{
			return _typeAccessor;
		}

		public Member GetMember(string name)
		{
			return _members.Single(x => x.Name.ToLower() == name.ToLower());
		}
	}
}
