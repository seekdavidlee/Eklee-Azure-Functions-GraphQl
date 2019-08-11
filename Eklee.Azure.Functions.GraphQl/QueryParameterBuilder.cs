using FastMember;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.Types;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository.InMemory;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryParameterBuilder<TSource>
	{
		private readonly QueryBuilder<TSource> _queryBuilder;
		private readonly IInMemoryComparerProvider _inMemoryComparerProvider;
		private readonly ModelConvention<TSource> _modelConvention = new ModelConvention<TSource>();
		private readonly List<ModelMember> _modelMemberList = new List<ModelMember>();
		private readonly List<QueryStep> _querySteps = new List<QueryStep>();
		private readonly QueryStep _queryStep;

		public QueryStep NewQueryStep()
		{
			return new QueryStep
			{
				ContextAction = ctx => ctx.SetResults(ctx.GetQueryResults<TSource>())
			};
		}

		public string GetQueryBuilderQueryName()
		{
			return _queryBuilder.QueryName;
		}

		public QueryParameterBuilder(QueryBuilder<TSource> queryBuilder, IInMemoryComparerProvider inMemoryComparerProvider)
		{
			_queryBuilder = queryBuilder;
			_inMemoryComparerProvider = inMemoryComparerProvider;
			_queryStep = NewQueryStep();
		}

		public QueryParameterBuilder<TSource> WithKeys()
		{
			_modelConvention.ModelType.ForEach(member =>
			{
				if ((KeyAttribute)member.GetAttribute(typeof(KeyAttribute), false) != null)
				{
					Add(false, member);
				}
			});

			return this;
		}

		private void Add(bool isOptional, Member member)
		{
			var modelMember = new ModelMember(typeof(TSource),
				_modelConvention.ModelType.GetTypeAccessor(), member, isOptional);

			_modelMemberList.Add(modelMember);
			_queryStep.QueryParameters.Add(new QueryParameter { MemberModel = modelMember });
		}

		/// <summary>
		/// Returns a clone copy of the stored query steps. We have to return a new instance
		/// given the QueryStep itself stored here is singleton.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		internal List<QueryStep> GetQuerySteps(ResolveFieldContext<object> context)
		{
			if (_queryStep.QueryParameters.Count > 0)
			{
				var queryStep = _queryStep.CloneQueryStep();

				queryStep.QueryParameters.ForEach(qsqp =>
				{
					qsqp.ContextValue = context.GetContextValue(qsqp.MemberModel, qsqp.Rule);
				});
				return new List<QueryStep> { queryStep };
			}

			var clonedList = _querySteps.Select(x => x.CloneQueryStep()).ToList();

			clonedList.ToList().ForEach(queryStep =>
			{
				var first = queryStep.Mapper != null && !queryStep.ForceCreateContextValueIfNull;
				queryStep.QueryParameters.ForEach(queryParameter =>
				{
					if (first)
					{
						first = false;
					}
					else
					{
						if (queryParameter.ContextValue == null)
							queryParameter.ContextValue = context.GetContextValue(queryParameter.MemberModel, queryParameter.Rule);
					}
				});

				if (queryStep.InMemoryFilterQueryParameters != null)
				{
					queryStep.InMemoryFilterQueryParameters.ForEach(queryParameter =>
						queryParameter.ContextValue = context.GetContextValue(queryParameter.MemberModel, queryParameter.Rule));
				}
			});

			return clonedList;
		}

		public void ForEach(Action<ModelMember> action)
		{
			_modelMemberList.ForEach(action);
		}

		public QueryParameterBuilder<TSource> WithProperty(Expression<Func<TSource, object>> expression, bool isOptional = false)
		{
			// The property access might be getting converted to object to match the func.
			// If so, get the operand and see if that's a member expression.
			MemberExpression memberExpression = expression.Body as MemberExpression ?? (expression.Body as UnaryExpression)?.Operand as MemberExpression;

			if (memberExpression != null)
			{
				// Find the member.
				var rawMemberExpression = memberExpression.ToString();
				var depth = rawMemberExpression.Count(x => x == '.');

				if (depth > 1)
				{
					throw new InvalidOperationException("WithProperty is used directly on the type properties and cannot include hierarchy because it then needs mapping. Consider using BeginWithProperty.");
				}

				Add(isOptional, _modelConvention.ModelType.GetMember(memberExpression.Member.Name));
			}

			return this;
		}

		public QueryStepBuilder<TSource, TProperty> BeginQuery<TProperty>()
		{
			return new QueryStepBuilder<TSource, TProperty>(this);
		}

		public QueryParameterBuilder<TSource> BeginWithProperty<TProperty>(Expression<Func<TProperty, object>> expression,
			Action<QueryExecutionContext> contextAction)
		{
			Add(expression, null, contextAction);
			return this;
		}

		public QueryStepBuilder<TSource, TProperty> ThenWithQuery<TProperty>()
		{
			return new QueryStepBuilder<TSource, TProperty>(this);
		}

		public QueryParameterBuilder<TSource> ThenWithProperty<TProperty>(
			Expression<Func<TProperty, object>> expression,
			Func<QueryExecutionContext, List<object>> mapper, Action<QueryExecutionContext> contextAction)
		{
			Add(expression, mapper, contextAction);
			return this;
		}

		private void Add<TProperty>(Expression<Func<TProperty, object>> expression,
			Func<QueryExecutionContext, List<object>> mapper,
			Action<QueryExecutionContext> contextAction)
		{
			Add(new List<Expression<Func<TProperty, object>>> { expression }, mapper, contextAction, null, false, null);
		}

		internal void Add<TProperty>(
			List<Expression<Func<TProperty, object>>> expressions,
			Func<QueryExecutionContext, List<object>> mapper,
			Action<QueryExecutionContext> contextAction,
			Dictionary<string, object> stepBagItems,
			bool skipConnectionEdgeCheck,
			Type overrideRepositoryWithType)
		{
			var step = new QueryStep
			{
				ContextAction = contextAction,
				Items = stepBagItems,
				OverrideRepositoryWithType = overrideRepositoryWithType,
				SkipConnectionEdgeCheck = skipConnectionEdgeCheck
			};

			if (mapper != null)
			{
				step.Mapper = ctx => mapper(ctx.Context);
			}

			bool first = mapper != null;

			expressions.ForEach(expression =>
			{
				// The property access might be getting converted to object to match the func.
				// If so, get the operand and see if that's a member expression.
				MemberExpression memberExpression = expression.Body as MemberExpression ?? (expression.Body as UnaryExpression)?.Operand as MemberExpression;

				if (memberExpression != null)
				{
					// Find the member.
					var rawMemberExpression = memberExpression.ToString();
					var depth = rawMemberExpression.Count(x => x == '.');
					string path = depth > 1 ? rawMemberExpression.Substring(rawMemberExpression.IndexOf('.') + 1) : memberExpression.Member.Name;

					var accessor = TypeAccessor.Create(typeof(TProperty));
					var member = accessor.GetMembers().ToList().Single(x =>
						x.Name == (depth > 1 ? path.Substring(0, path.IndexOf('.')) : memberExpression.Member.Name));

					var modelMember = new ModelMember(typeof(TProperty), accessor, member, false);

					if (first)
					{
						first = false;
					}
					else
					{
						_modelMemberList.Add(modelMember);
					}

					step.QueryParameters.Add(new QueryParameter
					{
						MemberModel = modelMember
					});
				}
				else
				{
					throw new ArgumentException("Expression provided is not a member expression.");
				}
			});

			_querySteps.Add(step);

		}

		public ConnectionEdgeQueryBuilder<TSource, TConnectionType> WithConnectionEdgeBuilder<TConnectionType>()
		{
			return new ConnectionEdgeQueryBuilder<TSource, TConnectionType>(this, _querySteps, _modelMemberList, _inMemoryComparerProvider);
		}

		public QueryBuilder<TSource> BuildQuery()
		{
			return _queryBuilder;
		}
	}
}
