using GraphQL.Builders;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class ModelMemberQueryArgumentProvider : IModelMemberQueryArgumentProvider
	{
		private readonly List<IModelMemberQueryArgument> _modelMemberQueryArguments;

		public ModelMemberQueryArgumentProvider(IEnumerable<IModelMemberQueryArgument> modelMemberQueryArguments)
		{
			_modelMemberQueryArguments = modelMemberQueryArguments.ToList();
		}

		public QueryArguments GetQueryArguments(IEnumerable<ModelMember> modelMembers)
		{
			var list = new List<QueryArgument>();

			modelMembers.ToList().ForEach(m =>
			{
				var handler = _modelMemberQueryArguments.SingleOrDefault(qa => qa.CanHandle(m));

				if (handler == null)
					throw new NotImplementedException($"QueryArgument type is not yet implemented for {m.Name}");

				list.AddRange(handler.GetArguments(m));
			});

			return new QueryArguments(list);
		}

		public void PopulateConnectionBuilder<TSource>(
			ConnectionBuilder<ModelConventionType<TSource>, object> connectionBuilder,
			IEnumerable<ModelMember> modelMembers)
		{
			modelMembers.ToList().ForEach(m =>
			{
				var handler = _modelMemberQueryArguments.SingleOrDefault(qa => qa.CanHandle(m));

				if (handler == null)
					throw new NotImplementedException($"QueryArgument type is not yet implemented for {m.Name}");

				handler.GetConnectionBuilderArguments(m, connectionBuilder)
					.ToList()
					.ForEach(arg => connectionBuilder = arg);
			});
		}
	}
}
