using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository
{
	[Trait(Constants.Category, Constants.IntegrationTests)]
	public class DocumentDbRepositoryTests : DocumentDbRepositoryTestsBase
	{
		[Fact]
		public async Task CanHandleDocumentTypeWithStringBasedPartition()
		{
			Expression<Func<DocumentDbFoo1, string>> expression = x => x.MyStringCategory;
			var configurations = GetBaseConfigurations<DocumentDbFoo1>((MemberExpression)expression.Body);

			DocumentDbRepository.Configure(typeof(DocumentDbFoo1), configurations);

			const string id = "3";

			await DocumentDbRepository.AddAsync(new DocumentDbFoo1
			{
				Id = id,
				Name = "Foo 1",
				MyStringCategory = "cat 1"
			}, null);

			await DocumentDbRepository.UpdateAsync(new DocumentDbFoo1
			{
				Id = id,
				Name = "Foo 1 v2",
				MyStringCategory = "cat 1"
			}, null);

			var item = (await GetByIdAsync<DocumentDbFoo1>(id)).Single();

			item.Name.ShouldBe("Foo 1 v2");

			DocumentDbRepository.DeleteAllAsync<DocumentDbFoo1>(null).GetAwaiter().GetResult();
		}

		[Fact]
		public async Task CanHandleDocumentTypeWithIntBasedPartition()
		{
			Expression<Func<DocumentDbFoo2, int>> expression = x => x.MyIntCategory;
			var configurations = GetBaseConfigurations<DocumentDbFoo2>((MemberExpression)expression.Body);

			DocumentDbRepository.Configure(typeof(DocumentDbFoo2), configurations);

			const string id = "2";

			await DocumentDbRepository.AddAsync(new DocumentDbFoo2
			{
				Id = id,
				Name = "Foo 2",
				MyIntCategory = 2
			}, null);

			await DocumentDbRepository.UpdateAsync(new DocumentDbFoo2
			{
				Id = id,
				Name = "Foo 2 v2",
				MyIntCategory = 2
			}, null);

			var item = (await GetByIdAsync<DocumentDbFoo2>(id)).Single();

			item.Name.ShouldBe("Foo 2 v2");

			await DocumentDbRepository.DeleteAllAsync<DocumentDbFoo2>(null);
		}

		[Fact]
		public async Task CanHandleDocumentTypeWithGuidBasedPartition()
		{
			Expression<Func<DocumentDbFoo4, Guid>> expression = x => x.MyGuidCategory;
			var configurations = GetBaseConfigurations<DocumentDbFoo4>((MemberExpression)expression.Body);

			DocumentDbRepository.Configure(typeof(DocumentDbFoo4), configurations);

			const string id = "5";
			Guid guid = Guid.NewGuid();

			await DocumentDbRepository.AddAsync(new DocumentDbFoo4
			{
				Id = id,
				Name = "Foo 4",
				MyGuidCategory = guid
			}, null);

			await DocumentDbRepository.UpdateAsync(new DocumentDbFoo4
			{
				Id = id,
				Name = "Foo 4 v2",
				MyGuidCategory = guid
			}, null);

			var item = (await GetByIdAsync<DocumentDbFoo4>(id)).Single();

			item.Name.ShouldBe("Foo 4 v2");

			await DocumentDbRepository.DeleteAllAsync<DocumentDbFoo4>(null);
		}

		[Fact]
		public async Task CanDeleteWithPartition()
		{
			Expression<Func<DocumentDbFoo2, int>> expression = x => x.MyIntCategory;
			var configurations = GetBaseConfigurations<DocumentDbFoo2>((MemberExpression)expression.Body);

			DocumentDbRepository.Configure(typeof(DocumentDbFoo2), configurations);

			const string id = "12";
			const int partition = 2;

			await DocumentDbRepository.AddAsync(new DocumentDbFoo2
			{
				Id = id,
				Name = "Foo 2A",
				MyIntCategory = partition
			}, null);

			await DocumentDbRepository.DeleteAsync(new DocumentDbFoo2 { Id = id, MyIntCategory = partition }, null);

			var exist = (await GetByIdAsync<DocumentDbFoo2>(id)).Any();

			exist.ShouldBe(false);

			await DocumentDbRepository.DeleteAllAsync<DocumentDbFoo2>(null);
		}

		[Fact]
		public async Task CanDeleteWithoutPartition()
		{
			Expression<Func<DocumentDbFoo2, int>> expression = x => x.MyIntCategory;
			var configurations = GetBaseConfigurations<DocumentDbFoo2>((MemberExpression)expression.Body);

			DocumentDbRepository.Configure(typeof(DocumentDbFoo2), configurations);

			const string id = "15";
			const int partition = 12;

			await DocumentDbRepository.AddAsync(new DocumentDbFoo2
			{
				Id = id,
				Name = "Foo 2A",
				MyIntCategory = partition
			}, null);

			await DocumentDbRepository.DeleteAsync(new DocumentDbFoo2 { Id = id }, null);

			var exist = (await GetByIdAsync<DocumentDbFoo2>(id)).Any();

			exist.ShouldBe(false);

			await DocumentDbRepository.DeleteAllAsync<DocumentDbFoo2>(null);
		}
	}
}
