using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository
{
	[Trait(Constants.Category, Constants.IntegrationTests)]
	public class DocumentDbRepositoryQueryTests : DocumentDbRepositoryTestsBase, IDisposable
	{
		private readonly Type _type = typeof(DocumentDbFoo3);
		private readonly TypeAccessor _accessor;
		private readonly MemberSet _members;

		public DocumentDbRepositoryQueryTests()
		{
			Expression<Func<DocumentDbFoo3, string>> expression = x => x.Category;
			var configurations = GetBaseConfigurations<DocumentDbFoo3>((MemberExpression)expression.Body);
			DocumentDbRepository.Configure(typeof(DocumentDbFoo3), configurations);

			_accessor = TypeAccessor.Create(_type);
			_members = _accessor.GetMembers();
		}

		private async Task Seed()
		{
			var list = new List<DocumentDbFoo3>
			{
				new DocumentDbFoo3
				{
					Id = "1",
					Name = "Foo 1",
					Category = "cat 1",
					Description = "Ha ha ha",
					Level = 4,
					Effective = new DateTime(2017, 1, 1).ToUtc(),
					TypeId = Guid.Parse("AF359AD6-9B8E-43F7-B898-35CEB400051A")
				},
				new DocumentDbFoo3
				{
					Id = "2",
					Name = "Foo 2",
					Category = "cat 1",
					Description = "Ra ha ha",
					Level = 2,
					Effective = new DateTime(2018, 1, 1).ToUtc(),
					TypeId = Guid.Parse("9B522321-B689-43EC-A3DC-DC17EE2A42DD")
				},
				new DocumentDbFoo3
				{
					Id = "3",
					Name = "Foo 3",
					Category = "cat 2",
					Description = "Na ha ha",
					Level = 3,
					Effective = new DateTime(2016, 1, 1).ToUtc(),
					Expires = new DateTime(2018, 5, 1).ToUtc(),
					TypeId = Guid.Parse("CC562BC9-C2E5-46D8-B701-E5D57F5800B8")
				},
				new DocumentDbFoo3
				{
					Id = "4",
					Name = "Foo 4",
					Category = "cat 1",
					Description = "Ba ha Ba",
					Level = 2,
					Effective = new DateTime(2014, 12, 1).ToUtc(),
					TypeId = Guid.Parse("4AB97B79-37DB-4AB7-BC32-D3EB1780C8AF")
				},
				new DocumentDbFoo3
				{
					Id = "5",
					Name = "Bar 5",
					Category = "cat 5",
					Description = "Sa Sa Ba",
					Level = 6,
					Effective = new DateTime(2014, 1, 1).ToUtc(),
					Expires = new DateTime(2015, 12, 31).ToUtc(),
					TypeId = Guid.Parse("9B522321-B689-43EC-A3DC-DC17EE2A42DD")
				}
			};

			foreach (var item in list)
			{
				await DocumentDbRepository.AddAsync(item, null);
			}
		}

		[Fact]
		public async Task CanQueryWithStartsWith()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Value = "Foo",
						Comparison = Comparisons.StringStartsWith },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Name"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(4);
			results[0].Name.ShouldStartWith("Foo");
			results[1].Name.ShouldStartWith("Foo");
			results[2].Name.ShouldStartWith("Foo");
			results[3].Name.ShouldStartWith("Foo");
		}

		[Fact]
		public async Task CanQueryWithEndsWith()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Value = "ha ha",
						Comparison = Comparisons.StringEndsWith},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Description"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);
			results[0].Description.ShouldEndWith("ha ha");
			results[1].Description.ShouldEndWith("ha ha");
			results[2].Description.ShouldEndWith("ha ha");
		}

		[Fact]
		public async Task CanQueryWithContains()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Value = "ha",
						Comparison = Comparisons.StringContains },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Description"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(4);
			results[0].Description.ShouldContain("ha");
			results[1].Description.ShouldContain("ha");
			results[2].Description.ShouldContain("ha");
			results[3].Description.ShouldContain("ha");
		}

		[Fact]
		public async Task CanQueryWithTwoArgsOfTypesStringAndInt()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = "cat 1",
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Category"), false)
				},
				new QueryParameter
				{
					ContextValue = new ContextValue {
						Value = 2,
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);

			var item2 = results.Single(x => x.Id == "2");
			item2.Category.ShouldBe("cat 1");
			item2.Level.ShouldBe(2);

			var item4 = results.Single(x => x.Id == "4");
			item4.Category.ShouldBe("cat 1");
			item4.Level.ShouldBe(2);
		}


		[Fact]
		public async Task CanQueryByGuid()
		{
			await Seed();

			var typeId = Guid.Parse("AF359AD6-9B8E-43F7-B898-35CEB400051A");
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = typeId,
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "TypeId"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);

			var item2 = results.Single(x => x.Id == "1");
			item2.Category.ShouldBe("cat 1");
			item2.TypeId.ShouldBe(typeId);
		}

		[Fact]
		public async Task CanQueryByDateTimeWithEqual()
		{
			await Seed();

			var date = DateTime.Parse("2018-01-01");
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = date,
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Effective"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);

			var item2 = results.Single(x => x.Id == "2");
			item2.Category.ShouldBe("cat 1");
			item2.Effective.ShouldBe(date);
		}

		[Fact]
		public async Task CanQueryByDateTimeWithGreaterEqualThan()
		{
			await Seed();

			var date = DateTime.Parse("2017-01-01");
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = date,
						Comparison = Comparisons.GreaterEqualThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Effective"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);

			var item1 = results.Single(x => x.Id == "1");
			item1.Category.ShouldBe("cat 1");

			var item2 = results.Single(x => x.Id == "2");
			item2.Category.ShouldBe("cat 1");
		}

		[Fact]
		public async Task CanQueryByDateTimeWithGreaterThan()
		{
			await Seed();

			var date = DateTime.Parse("2017-01-01");
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = date,
						Comparison = Comparisons.GreaterThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Effective"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);

			var item2 = results.Single(x => x.Id == "2");
			item2.Category.ShouldBe("cat 1");
		}

		[Fact]
		public async Task CanQueryByDateTimeWithLessThan()
		{
			await Seed();

			var date = DateTime.Parse("2014-01-02");
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = date,
						Comparison = Comparisons.LessThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Effective"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);

			var item2 = results.Single(x => x.Id == "5");
			item2.Name.ShouldBe("Bar 5");
		}

		[Fact]
		public async Task CanQueryByDateTimeWithLessEqualThan()
		{
			await Seed();

			var date = DateTime.Parse("2014-12-01");
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = date,
						Comparison = Comparisons.LessEqualThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Effective"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);

			var item1 = results.Single(x => x.Id == "4");
			item1.Name.ShouldBe("Foo 4");

			var item2 = results.Single(x => x.Id == "5");
			item2.Name.ShouldBe("Bar 5");
		}

		[Fact]
		public async Task CanQueryByIntWithEqual()
		{
			await Seed();

			var level = 6;
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = level,
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);

			var item2 = results.Single(x => x.Id == "5");
			item2.Level.ShouldBe(level);
		}

		[Fact]
		public async Task CanQueryByIntWithGreaterThan()
		{
			await Seed();

			var level = 4;
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = level,
						Comparison = Comparisons.GreaterThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);

			var item2 = results.Single(x => x.Id == "5");
			item2.Level.ShouldBe(6);
		}

		[Fact]
		public async Task CanQueryByIntWithGreaterEqualThan()
		{
			await Seed();

			var level = 4;
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = level,
						Comparison = Comparisons.GreaterEqualThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);

			var item1 = results.Single(x => x.Id == "1");
			item1.Level.ShouldBe(4);

			var item2 = results.Single(x => x.Id == "5");
			item2.Level.ShouldBe(6);
		}

		[Fact]
		public async Task CanQueryByIntWithLessThan()
		{
			await Seed();

			var level = 3;
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = level,
						Comparison = Comparisons.LessThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);

			var item1 = results.Single(x => x.Id == "2");
			item1.Level.ShouldBe(2);

			var item2 = results.Single(x => x.Id == "4");
			item2.Level.ShouldBe(2);
		}

		[Fact]
		public async Task CanQueryByIntWithLessEqualThan()
		{
			await Seed();

			var level = 3;
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = level,
						Comparison = Comparisons.LessEqualThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);

			var item1 = results.Single(x => x.Id == "2");
			item1.Level.ShouldBe(2);

			var item2 = results.Single(x => x.Id == "4");
			item2.Level.ShouldBe(2);

			var item3 = results.Single(x => x.Id == "3");
			item3.Level.ShouldBe(3);
		}

		public void Dispose()
		{
			DocumentDbRepository.DeleteAllAsync<DocumentDbFoo3>(null).GetAwaiter().GetResult();
		}
	}
}
