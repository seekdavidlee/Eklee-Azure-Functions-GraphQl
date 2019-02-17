using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.TableStorage
{
	[Collection(Constants.TableStorageTests)]
	[Trait(Constants.Category, Constants.IntegrationTests)]
	public class TableStorageRepositoryTests : TableStorageRepositoryTestsBase
	{
		[Fact]
		public async Task CanHandleEntityTypeWithStringBasedPartition()
		{
			Expression<Func<DocumentDbFoo1, string>> expression = x => x.MyStringCategory;
			var configurations = GetBaseConfigurations<DocumentDbFoo1>((MemberExpression)expression.Body);

			TableStorageRepository.Configure(typeof(DocumentDbFoo1), configurations);

			const string id = "3";

			await TableStorageRepository.AddAsync(new DocumentDbFoo1
			{
				Id = id,
				Name = "Foo 1",
				MyStringCategory = "cat 1"
			}, null);

			await TableStorageRepository.UpdateAsync(new DocumentDbFoo1
			{
				Id = id,
				Name = "Foo 1 v2",
				MyStringCategory = "cat 1"
			}, null);

			try
			{
				var item = (await GetByIdAsync<DocumentDbFoo1>(id)).Single();

				item.Name.ShouldBe("Foo 1 v2");
			}
			finally
			{
				TableStorageRepository.DeleteAllAsync<DocumentDbFoo1>(null).GetAwaiter().GetResult();
			}
		}
	}
}
