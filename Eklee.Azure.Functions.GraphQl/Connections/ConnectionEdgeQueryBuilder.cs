using FastMember;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdgeQueryBuilder<TSource, TConnectionType>
	{
		private readonly QueryParameterBuilder<TSource> _source;
		private readonly List<QueryStep> _querySteps;
		private readonly List<ModelMember> _modelMemberList;

		private bool _withDestinationId;

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

		public QueryParameterBuilder<TSource> BuildConnectionEdgeParameters()
		{
			_querySteps.Add(QueryConnectionEdge());
			_querySteps.Add(QuerySource());

			return _source;
		}

		private QueryStep QuerySource()
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

			queryStep.Mapper = (ctx) =>
			{
				var connectionEdges = ctx.GetQueryResults<ConnectionEdge>();
				return connectionEdges.Select(x => (object)x.SourceId).ToList();
			};
			return queryStep;
		}

		private QueryStep QueryConnectionEdge()
		{
			var queryStep = new QueryStep
			{
				ContextAction = ctx => ctx.SetResults(ctx.GetQueryResults<ConnectionEdge>())
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

			if (_withDestinationId)
			{
				var destinationIdMember = new ModelMember(type, typeAccessor,
						members.Single(x => x.Name == "DestinationId"), false);
				queryStep.QueryParameters.Add(new QueryParameter
				{
					MemberModel = destinationIdMember,
					Rule = new ContextValueSetRule
					{
						DisableSetSelectValues = true
					}
				});

				_modelMemberList.Add(destinationIdMember);
			}
			return queryStep;
		}
	}
}
