using FastMember;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryParameterBuilder<TSource>
	{
		private readonly QueryBuilder<TSource> _queryBuilder;
		private readonly ModelConvention<TSource> _modelConvention = new ModelConvention<TSource>();
		private readonly List<ModelMember> _modelMemberList = new List<ModelMember>();
		private readonly List<QueryStep> _querySteps = new List<QueryStep>();
		private readonly QueryStep _queryStep = new QueryStep
		{
			ContextAction = ctx => ctx.SetResults(ctx.GetQueryResults<TSource>())
		};

		public QueryParameterBuilder(QueryBuilder<TSource> queryBuilder)
		{
			_queryBuilder = queryBuilder;
		}

		public QueryParameterBuilder<TSource> WithKeys()
		{
			_modelConvention.ModelType.ForEach(member =>
			{
				if ((KeyAttribute)member.GetAttribute(typeof(KeyAttribute), false) != null)
				{
					Add(false, member, Comparisons.Equals);
				}
			});

			return this;
		}

		private void Add(bool isOptional, Member member, Comparisons comparison)
		{
			var modelMember = new ModelMember(typeof(TSource), _modelConvention.ModelType.GetTypeAccessor(), member, isOptional);
			_modelMemberList.Add(modelMember);
			_queryStep.QueryParameters.Add(new QueryParameter { MemberModel = modelMember, Comparison = comparison, HasContextValue = true });
		}

		public List<QueryStep> GetQuerySteps(ResolveFieldContext<object> context)
		{
			if (_queryStep.QueryParameters.Count > 0)
			{
				_queryStep.QueryParameters.ForEach(qsqp => qsqp.ContextValue = context.Arguments.GetContextValue(qsqp.MemberModel.Name));
				return new List<QueryStep> { _queryStep };
			}

			var qp = _querySteps.Single(x => x.QueryParameters.Single().HasContextValue).QueryParameters.Single();
			qp.ContextValue = context.Arguments.GetContextValue(qp.MemberModel.Name);
			return _querySteps;
		}

		public void ForEach(Action<ModelMember> action)
		{
			_modelMemberList.ForEach(action);
		}

		public QueryParameterBuilder<TSource> WithProperty(Expression<Func<TSource, object>> expression, Comparisons comparison, bool isOptional = false)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				// Find the member.
				var rawMemberExpression = memberExpression.ToString();
				var depth = rawMemberExpression.Count(x => x == '.');

				if (depth > 1)
				{
					throw new InvalidOperationException("WithProperty is used directly on the type properties and cannot include hierarchy because it then needs mapping. Consider using BeginWithProperty.");
				}

				Add(isOptional, _modelConvention.ModelType.GetMember(memberExpression.Member.Name), comparison);
			}

			return this;
		}

		public QueryParameterBuilder<TSource> BeginWithProperty<TProperty>(Expression<Func<TProperty, object>> expression, Comparisons comparison,
			Action<QueryExecutionContext> contextAction)
		{
			Add(expression, comparison, null, contextAction, true);
			return this;
		}

		public QueryParameterBuilder<TSource> ThenWithProperty<TProperty>(Expression<Func<TProperty, object>> expression, Comparisons comparison,
			Func<QueryExecutionContext, List<object>> mapper, Action<QueryExecutionContext> contextAction)
		{
			Add(expression, comparison, mapper, contextAction, false);
			return this;
		}

		private void Add<TProperty>(Expression<Func<TProperty, object>> expression, Comparisons comparison,
			Func<QueryExecutionContext, List<object>> mapper,
			Action<QueryExecutionContext> contextAction, bool hasContextValue)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				// Find the member.
				var rawMemberExpression = memberExpression.ToString();
				var depth = rawMemberExpression.Count(x => x == '.');
				string path = depth > 1 ? rawMemberExpression.Substring(rawMemberExpression.IndexOf('.') + 1) : memberExpression.Member.Name;

				var accessor = TypeAccessor.Create(typeof(TProperty));
				var member = accessor.GetMembers().ToList().Single(x =>
					x.Name == (depth > 1 ? path.Substring(0, path.IndexOf('.')) : memberExpression.Member.Name));

				var modelMember = new ModelMember(typeof(TProperty), accessor, member, false);

				if (hasContextValue) _modelMemberList.Add(modelMember);

				var step = new QueryStep
				{
					ContextAction = contextAction,
					Mapper = mapper
				};

				step.QueryParameters.Add(new QueryParameter
				{
					MemberModel = modelMember,
					Comparison = comparison,
					HasContextValue = hasContextValue
				});

				_querySteps.Add(step);
			}
		}

		public QueryBuilder<TSource> Build()
		{
			return _queryBuilder;
		}
	}

	public enum Comparisons
	{
		Equals,
		StringContains,
		StringStartsWith,
		StringEndsWith
	}
}
