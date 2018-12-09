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
					Add(m.Name, false, m);
				}
			});

			return this;
		}

		private void Add(string path, bool isOptional, Member member)
		{
			_modelMemberList.Add(new ModelMember(_modelConvention.ModelType.GetTypeAccessor(), path, member, isOptional));
		}

		public IEnumerable<QueryParameter> GetQueryParameterList(Func<string, ContextValue> func)
		{
			return _modelMemberList.Select(modelMember =>
			{
				var queryParameter = new QueryParameter
				{
					ContextValue = func(modelMember.Name),
					MemberModel = modelMember
				};
				return queryParameter;
			});
		}

		public void ForEach(Action<ModelMember> action)
		{
			_modelMemberList.ForEach(action);
		}

		public QueryParameterBuilder<TSource> WithProperty<TProperty>(Expression<Func<TSource, TProperty>> expression, bool isOptional = false)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				// Find the member.

				var rawMemberExpression = memberExpression.ToString();
				var depth = rawMemberExpression.Count(x => x == '.');
				string path = depth > 1 ? rawMemberExpression.Substring(rawMemberExpression.IndexOf('.') + 1) : memberExpression.Member.Name;

				Add(path, isOptional, _modelConvention.ModelType.GetMember(depth > 1 ? 
					path.Substring(0, path.IndexOf('.')) : 
					memberExpression.Member.Name));
			}

			return this;
		}

		public QueryBuilder<TSource> Build()
		{
			return _queryBuilder;
		}
	}
}
