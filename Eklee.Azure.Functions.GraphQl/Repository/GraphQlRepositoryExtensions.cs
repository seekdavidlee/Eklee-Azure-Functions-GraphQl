using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public static class GraphQlRepositoryExtensions
	{
		public static async Task DeleteAllAsync(this IGraphQlRepository graphQlRepository, Type type)
		{
			MethodInfo method = graphQlRepository.GetType().GetMethod("DeleteAllAsync");

			MethodInfo generic = method.MakeGenericMethod(type);

			var task = (Task)generic.Invoke(graphQlRepository, null);

			await task.ConfigureAwait(false);
		}

		public static async Task BatchAddAsync(this IGraphQlRepository graphQlRepository, Type type, IEnumerable<object> items)
		{
			MethodInfo method = graphQlRepository.GetType().GetMethod("BatchAddAsync");

			MethodInfo generic = method.MakeGenericMethod(type);

			var task = (Task)generic.Invoke(graphQlRepository, new object[] { items });

			await task.ConfigureAwait(false);
		}

		public static async Task DeleteAsync(this IGraphQlRepository graphQlRepository, Type type,
			object mappedInstance)
		{
			await RunAsync(graphQlRepository, type, mappedInstance, "DeleteAsync");
		}

		public static async Task AddAsync(this IGraphQlRepository graphQlRepository, Type type, object mappedInstance)
		{
			await RunAsync(graphQlRepository, type, mappedInstance, "AddAsync");
		}

		public static async Task UpdateAsync(this IGraphQlRepository graphQlRepository, Type type, object mappedInstance)
		{
			await RunAsync(graphQlRepository, type, mappedInstance, "UpdateAsync");
		}

		private static async Task RunAsync(IGraphQlRepository graphQlRepository, Type type, object mappedInstance, string action)
		{
			MethodInfo method = graphQlRepository.GetType().GetMethod(action);

			MethodInfo generic = method.MakeGenericMethod(type);

			var task = (Task)generic.Invoke(graphQlRepository, new[] { mappedInstance });

			await task.ConfigureAwait(false);
		}
	}
}
