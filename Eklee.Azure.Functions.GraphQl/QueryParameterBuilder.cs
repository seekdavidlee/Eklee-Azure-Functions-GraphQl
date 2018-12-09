using FastMember;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryParameterBuilder<TSource>
	{
		private readonly QueryBuilder<TSource> _queryBuilder;
		private readonly ModelConvention<TSource> _modelConvention = new ModelConvention<TSource>();
		private readonly List<ModelMember> _modelMemberList = new List<ModelMember>();

		public QueryParameterBuilder(QueryBuilder<TSource> queryBuilder)
		{
			_queryBuilder = queryBuilder;
		}

		public QueryParameterBuilder<TSource> WithKeys()
		{
			_modelConvention.ModelType.ForEach(m =>
			{
				if ((KeyAttribute)m.GetAttribute(typeof(KeyAttribute), false) != null)
				{
					Add(m.Name, false);
				}
			});

			return this;
		}

		private void Add(string name, bool isOptional)
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

		public QueryParameterBuilder<TSource> WithProperty<TProperty>(Expression<Func<TSource, TProperty>> expression, bool isOptional = false)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				Add(memberExpression.Member.Name, isOptional);
			}

			return this;
		}

		public QueryBuilder<TSource> Build()
		{
			return _queryBuilder;
		}
	}
}
