using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.TableStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.TableStorage
{
	public abstract class TableStorageRepositoryTestsBase
	{
		protected readonly TableStorageRepository TableStorageRepository;

		protected TableStorageRepositoryTestsBase()
		{
			TableStorageRepository = new TableStorageRepository(Substitute.For<ILogger>(), new List<ITableStorageComparison>
			{
				new TableStorageComparisonBool(),
				new TableStorageComparisonInt(),
				new TableStorageComparisonString(),
				new TableStorageComparisonDate(),
				new TableStorageComparisonGuid()
			});
		}
		protected Dictionary<string, object> GetBaseConfigurations<TSource>(MemberExpression memberExpression)
		{
			var configurations = new Dictionary<string, object>();

			configurations.Add<TSource>(TableStorageConstants.ConnectionString, "UseDevelopmentStorage=true");
			configurations.Add<TSource>(TableStorageConstants.PartitionMemberExpression, memberExpression);


			return configurations;
		}

		protected async Task<IEnumerable<T>> GetByIdAsync<T>(string id) where T : class
		{
			var type = typeof(T);
			var accessor = TypeAccessor.Create(type);
			var member = accessor.GetMembers().Single(x => x.Name == "Id");

			return await TableStorageRepository.QueryAsync<T>("test1", new[]
			{
				new QueryParameter
				{
					ContextValue = new ContextValue { Value = id, Comparison = Comparisons.Equal},
					MemberModel = new ModelMember(type, accessor, member, false)
				}
			}, null, null);
		}
	}
}