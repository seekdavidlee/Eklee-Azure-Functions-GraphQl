using System;
using System.Collections;
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

			// ReSharper disable once PossibleNullReferenceException
			MethodInfo generic = method.MakeGenericMethod(type);

			var task = (Task)generic.Invoke(graphQlRepository, null);

			await task.ConfigureAwait(false);
		}

		public static async Task BatchAddAsync(this IGraphQlRepository graphQlRepository, Type type, List<object> items)
		{
			var listType = typeof(List<>).MakeGenericType(type);
			var list = Activator.CreateInstance(listType);
			var c = (IList)list;
			items.ForEach(item => c.Add(item));

			MethodInfo method = graphQlRepository.GetType().GetMethod("BatchAddAsync");

			// ReSharper disable once PossibleNullReferenceException
			MethodInfo generic = method.MakeGenericMethod(type);

			var task = (Task)generic.Invoke(graphQlRepository, new[] { list });

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

			// ReSharper disable once PossibleNullReferenceException
			MethodInfo generic = method.MakeGenericMethod(type);

			var task = (Task)generic.Invoke(graphQlRepository, new[] { mappedInstance });

			await task.ConfigureAwait(false);
		}
	}
}
