using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using FastMember;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository
{
	public class InMemoryBar1
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class InMemoryRepositoryQueryTests
	{
		private readonly InMemoryRepository _inMemoryRepository;
		private readonly Type _type = typeof(InMemoryBar1);
		private readonly TypeAccessor _accessor;
		private readonly MemberSet _members;

		public InMemoryRepositoryQueryTests()
		{
			_inMemoryRepository = new InMemoryRepository();
			_accessor = TypeAccessor.Create(_type);
			_members = _accessor.GetMembers();
		}

		private async Task Seed()
		{
			var list = new List<InMemoryBar1>();
			list.Add(new InMemoryBar1
			{
				Id = 1,
				Name = "Bar 1",
				Category = "NB"
			});

			list.Add(new InMemoryBar1
			{
				Id = 2,
				Name = "Bar 2",
				Category = "NB"
			});

			list.Add(new InMemoryBar1
			{
				Id = 3,
				Name = "Bar 3",
				Category = "FB"
			});

			list.Add(new InMemoryBar1
			{
				Id = 4,
				Name = "Bar 4",
				Category = "CB"
			});

			list.Add(new InMemoryBar1
			{
				Id = 5,
				Name = "Bar 5",
				Category = "FX"
			});

			foreach (var inMemoryBar1 in list)
			{
				await _inMemoryRepository.AddAsync(inMemoryBar1);
			}
		}

		[Fact]
		public async Task CanQueryWith_StartsWith()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Value = "Bar",
						Comparison = Comparisons.StringStartsWith },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Name"), false)
				}
			};

			var results = (await _inMemoryRepository.QueryAsync<InMemoryBar1>("test", args, null)).ToList();

			results.Count.ShouldBe(5);

			results[0].Name.ShouldStartWith("Bar");
			results[1].Name.ShouldStartWith("Bar");
			results[2].Name.ShouldStartWith("Bar");
			results[3].Name.ShouldStartWith("Bar");
			results[4].Name.ShouldStartWith("Bar");
		}
	}
}
