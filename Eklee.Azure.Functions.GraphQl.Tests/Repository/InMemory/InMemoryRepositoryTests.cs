using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using FastMember;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.InMemory
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class InMemoryRepositoryTests
	{
		private readonly InMemoryRepository _inMemoryRepository;
		private readonly Type _type = typeof(InMemoryBar1);
		private readonly TypeAccessor _accessor;
		private readonly MemberSet _members;

		public InMemoryRepositoryTests()
		{
			var list = new List<IInMemoryCompare> { new InMemoryCompareInt(), new InMemoryCompareString() };
			_inMemoryRepository = new InMemoryRepository(new InMemoryComparerProvider(list));
			_accessor = TypeAccessor.Create(_type);
			_members = _accessor.GetMembers();
		}

		[Fact]
		public async Task CanAddOrUpdateItems()
		{
			var item = new InMemoryBar1();
			item.Id = 1;
			item.Name = "aa";
			item.Category = "bb";

			await _inMemoryRepository.AddOrUpdateAsync(item, null);

			var itemAgain = new InMemoryBar1();
			itemAgain.Id = 1;
			itemAgain.Name = "cc";
			itemAgain.Category = "dd";

			await _inMemoryRepository.AddOrUpdateAsync(itemAgain, null);

			var updatedItem = await GetById(1);
			updatedItem.Name.ShouldBe("cc");
			updatedItem.Category.ShouldBe("dd");
		}

		private async Task<InMemoryBar1> GetById(int id)
		{
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{ id},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Id"), false)
				}
			};

			return (await _inMemoryRepository.QueryAsync<InMemoryBar1>("ff", args, null, null)).SingleOrDefault();
		}
	}
}
