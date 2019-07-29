﻿using FastMember;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdgeQueryBuilder<TSource, TConnectionType>
	{
		private readonly QueryParameterBuilder<TSource> _source;
		private readonly List<QueryStep> _querySteps;
		private readonly List<ModelMember> _modelMemberList;
		private bool _withDestinationId;
		private bool _withDestinationIdFromSource;

		public ConnectionEdgeQueryBuilder(QueryParameterBuilder<TSource> source,
			List<QueryStep> querySteps,
			List<ModelMember> modelMemberList)
		{
			_source = source;
			_querySteps = querySteps;
			_modelMemberList = modelMemberList;
		}

		public ConnectionEdgeQueryBuilder<TSource, TConnectionType> WithDestinationId()
		{
			_withDestinationId = true;
			return this;
		}

		public QueryParameterBuilder<TSource> BuildConnectionEdgeParameters(Action<QueryExecutionContext> mapper = null)
		{
			_querySteps.Add(QueryConnectionEdge());
			_querySteps.Add(QuerySource(mapper));

			return _source;
		}

		private Func<QueryExecutionContext, List<object>> _mapper;
		public ConnectionEdgeQueryBuilder<TSource, TConnectionType> WithDestinationIdFromSource(Func<QueryExecutionContext, List<object>> mapper)
		{
			_mapper = mapper;
			_withDestinationIdFromSource = true;
			return this;
		}

		private readonly List<Expression<Func<TConnectionType, object>>> _expressions = new List<Expression<Func<TConnectionType, object>>>();
		public ConnectionEdgeQueryBuilder<TSource, TConnectionType> WithProperty(Expression<Func<TConnectionType, object>> expression)
		{
			_expressions.Add(expression);
			return this;
		}

		private readonly Type _type = typeof(TConnectionType);
		private readonly TypeAccessor _accessor = TypeAccessor.Create(typeof(TConnectionType));
		
		private void AddExpressions()
		{
			_expressions.ForEach(expression =>
			{
				MemberExpression memberExpression = expression.Body as MemberExpression ?? (expression.Body as UnaryExpression)?.Operand as MemberExpression;

				if (memberExpression != null)
				{
					// Find the member.
					var rawMemberExpression = memberExpression.ToString();
					var depth = rawMemberExpression.Count(x => x == '.');
					string path = depth > 1 ? rawMemberExpression.Substring(rawMemberExpression.IndexOf('.') + 1) : memberExpression.Member.Name;

					var member = _accessor.GetMembers().ToList().Single(x =>
						x.Name == (depth > 1 ? path.Substring(0, path.IndexOf('.')) : memberExpression.Member.Name));

					var modelMember = new ModelMember(_type, _accessor, member, false);

					_modelMemberList.Add(modelMember);
				}
			});

		}

		private QueryStep QuerySource(Action<QueryExecutionContext> mapper = null)
		{
			var queryStep = _source.NewQueryStep();

			queryStep.ForceCreateContextValueIfNull = true;

			var type = typeof(TSource);
			var typeAccessor = TypeAccessor.Create(type);
			var members = typeAccessor.GetMembers().ToList();

			var idMember = members.Single(x => x.GetAttribute(typeof(KeyAttribute), false) != null);

			queryStep.QueryParameters.Add(new QueryParameter
			{
				MemberModel = new ModelMember(type, typeAccessor, idMember, false)
			});

			AddExpressions();

			queryStep.Mapper = (ctx) =>
			{
				var connectionEdges = ctx.GetQueryResults<ConnectionEdge>();

				if (mapper != null)
				{
					var items = connectionEdges.Select(x => JsonConvert.DeserializeObject(
						x.MetaValue, Type.GetType(x.MetaType))).ToList();

					// Override result.
					ctx.SetQueryResult(items);

					mapper(ctx);
				}
				return connectionEdges.Select(x => (object)x.SourceId).ToList();
			};
			return queryStep;
		}

		private QueryStep QueryConnectionEdge()
		{
			var queryStep = new QueryStep
			{
				ContextAction = ctx => ctx.SetResults(ctx.GetQueryResults<ConnectionEdge>()),
				Mapper = _mapper
			};

			var type = typeof(ConnectionEdge);
			var typeAccessor = TypeAccessor.Create(type);
			var members = typeAccessor.GetMembers().ToList();

			queryStep.QueryParameters.Add(new QueryParameter
			{
				MemberModel = new ModelMember(type, typeAccessor,
					members.Single(x => x.Name == "MetaType"), false),
				ContextValue = new ContextValue
				{
					Comparison = Comparisons.Equal,
					Values = new List<object> { typeof(TConnectionType).AssemblyQualifiedName }
				}
			});

			queryStep.QueryParameters.Add(new QueryParameter
			{
				MemberModel = new ModelMember(type, typeAccessor,
					members.Single(x => x.Name == "SourceType"), false),
				ContextValue = new ContextValue
				{
					Comparison = Comparisons.Equal,
					Values = new List<object> { typeof(TSource).AssemblyQualifiedName }
				}
			});

			var destinationIdMember = new ModelMember(type, typeAccessor,
					members.Single(x => x.Name == "DestinationId"), false);

			queryStep.QueryParameters.Add(new QueryParameter
			{
				MemberModel = destinationIdMember,
				Rule = new ContextValueSetRule
				{
					DisableSetSelectValues = _withDestinationId,
					PopulateWithQueryValues = _withDestinationIdFromSource
				}
			});

			if (_withDestinationId)
				_modelMemberList.Add(destinationIdMember);

			return queryStep;
		}
	}
}
