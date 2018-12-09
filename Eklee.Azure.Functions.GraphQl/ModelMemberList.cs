using FastMember;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelMemberList<TSource>
	{
		private readonly ModelConvention<TSource> _modelConvention;
		private readonly List<ModelMember> _modelMemberList = new List<ModelMember>();

		public ModelMemberList(ModelConvention<TSource> modelConvention)
		{
			_modelConvention = modelConvention;
		}

		public void PopulateWithKeyAttribute()
		{
			_modelConvention.ModelType.ForEach(m =>
			{
				if ((KeyAttribute)m.GetAttribute(typeof(KeyAttribute), false) != null)
				{
					Add(m.Name, false);
				}
			});
		}

		public void Add(string name, bool isOptional)
		{
			_modelMemberList.Add(new ModelMember { Name = name.ToLower(), IsOptional = isOptional });
		}

		public IEnumerable<QueryParameter> GetQueryParameterList(Func<string, ContextValue> func)
		{
			return _modelMemberList.Select(memberSetup =>
			{
				var queryParameter = new QueryParameter
				{
					ContextValue = func(memberSetup.Name),
					Member = _modelConvention.ModelType.GetMember(memberSetup.Name),
					MemberParent = _modelConvention.ModelType.GetTypeAccessor(),
					IsOptional = memberSetup.IsOptional
				};
				return queryParameter;
			});
		}

		public void ForEach(Action<ModelMember, Member> action)
		{
			_modelMemberList.ForEach(x => action(x, _modelConvention.ModelType.GetMember(x.Name)));
		}
	}
}
