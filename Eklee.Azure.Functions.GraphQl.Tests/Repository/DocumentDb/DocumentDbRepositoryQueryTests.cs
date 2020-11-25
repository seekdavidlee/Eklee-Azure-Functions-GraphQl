using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.DocumentDb
{
	[Collection(Constants.DocumentDbTests)]
	[Trait(Constants.Category, Constants.IntegrationTests)]
	public class DocumentDbRepositoryQueryTests : DocumentDbRepositoryTestsBase, IDisposable
	{
		private readonly Type _type = typeof(DocumentDbFoo3);
		private readonly TypeAccessor _accessor;
		private readonly MemberSet _members;

		public DocumentDbRepositoryQueryTests()
		{
			Expression<Func<DocumentDbFoo3, string>> expression = x => x.Category;

			"DocumentDbRepositoryQueryTests instantiated and loading config.".Log();
			var configurations = GetBaseConfigurations<DocumentDbFoo3>((MemberExpression)expression.Body);

			"Configuring DocumentDbRepository.".Log();
			DocumentDbRepository.Configure(typeof(DocumentDbFoo3), configurations);

			"Creating DocumentDbRepositoryQueryTests accessor type.".Log();

			_accessor = TypeAccessor.Create(_type);
			_members = _accessor.GetMembers();

			"DocumentDbRepositoryQueryTests constructor.".Log();
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
					TypeId = Guid.Parse("AF359AD6-9B8E-43F7-B898-35CEB400051A"),
					IsActive = true
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
					TypeId = Guid.Parse("9B522321-B689-43EC-A3DC-DC17EE2A42DD"),
					IsActive = true
				},
				new DocumentDbFoo3
				{
					Id = "6",
					Name = "Nest 6",
					Category = "ele 6",
					Description = "JUST ALP",
					Level = 8,
					Effective = new DateTime(2019, 1, 1).ToUtc(),
					Expires = new DateTime(2020, 12, 31).ToUtc(),
					TypeId = Guid.Parse("DC742320-F3A4-4DE1-9239-6A9EC68BD430"),
					IsActive = true
				},
				new DocumentDbFoo3
				{
					Id = "7",
					Name = "Desk 7",
					Category = "Mal 7",
					Description = "KO PO LA",
					Level = 9,
					Effective = new DateTime(2019, 4, 1).ToUtc(),
					Expires = new DateTime(2020, 11, 5).ToUtc(),
					TypeId = Guid.Parse("D4FB1574-05CF-4422-9477-13D722A5448E"),
					IsActive = true
				},
				new DocumentDbFoo3
				{
					Id = "8",
					Name = "Desk 8",
					Category = "Pal 8",
					Description = "IN AS JA",
					Level = 9,
					Effective = new DateTime(2019, 4, 15).ToUtc(),
					Expires = new DateTime(2021, 11, 9).ToUtc(),
					TypeId = Guid.Parse("DE6EFD89-2D62-4E63-98D2-AEA9E622B223"),
					IsActive = true
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
					ContextValue = new ContextValue { Values = new List<object>{ "Foo"},
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
					ContextValue = new ContextValue { Values = new List<object>{  "ha ha"},
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
					ContextValue = new ContextValue { Values =new List<object>{ "ha"},
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
						Values =new List<object>{ "cat 1"},
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Category"), false)
				},
				new QueryParameter
				{
					ContextValue = new ContextValue {
						Values = new List<object>{ 2},
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
						Values =new List<object>{typeId},
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

			var date = DateTime.Parse("2018-01-01").ToUtc();
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{date},
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

			var date = DateTime.Parse("2019-04-01").ToUtc();
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ date},
						Comparison = Comparisons.GreaterEqualThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Effective"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);

			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryByDateTimeWithGreaterThan()
		{
			await Seed();

			var date = DateTime.Parse("2019-04-01").ToUtc();
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ date},
						Comparison = Comparisons.GreaterThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Effective"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryByDateTimeWithLessThan()
		{
			await Seed();

			var date = DateTime.Parse("2014-01-02").ToUtc();
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ date},
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

			var date = DateTime.Parse("2014-12-01").ToUtc();
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ date},
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
		public async Task CanQueryByBoolWithEqualFalse()
		{
			await Seed();


			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ false},
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "IsActive"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);

			results.Any(x => x.Id == "2").ShouldBe(true);
			results.Any(x => x.Id == "3").ShouldBe(true);
			results.Any(x => x.Id == "4").ShouldBe(true);
		}

		[Fact]
		public async Task CanQueryByBoolWithEqualTrue()
		{
			await Seed();


			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ true},
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "IsActive"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(5);

			results.Any(x => x.Id == "1").ShouldBeTrue();
			results.Any(x => x.Id == "5").ShouldBeTrue();
			results.Any(x => x.Id == "6").ShouldBeTrue();
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
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
						Values =new List<object>{ level},
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

			var level = 8;
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ level},
						Comparison = Comparisons.GreaterThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);

			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryByIntWithGreaterEqualThan()
		{
			await Seed();

			var level = 8;
			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ level},
						Comparison = Comparisons.GreaterEqualThan
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);

			results.Any(x => x.Id == "6").ShouldBeTrue();
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
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
						Values =new List<object>{ level},
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
						Values =new List<object>{ level},
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

		[Fact]
		public async Task CanQueryByMultipleStringsWithEqual()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ "Nest 6", "Desk 7" },
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Name"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);

			var item1 = results.Single(x => x.Id == "6");
			item1.Name.ShouldBe("Nest 6");

			var item2 = results.Single(x => x.Id == "7");
			item2.Name.ShouldBe("Desk 7");
		}

		[Fact]
		public async Task CanQueryByMultipleIntsWithEqual()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>{ 8, 9 },
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);

			var item1 = results.Single(x => x.Id == "6");
			item1.Level.ShouldBe(8);

			var item2 = results.Single(x => x.Id == "7");
			item2.Level.ShouldBe(9);

			var item3 = results.Single(x => x.Id == "8");
			item3.Level.ShouldBe(9);
		}

		[Fact]
		public async Task CanQueryByMultipleGuidsWithEqual()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Values =new List<object>
						{
							Guid.Parse("DC742320-F3A4-4DE1-9239-6A9EC68BD430"),
							Guid.Parse("D4FB1574-05CF-4422-9477-13D722A5448E"),
							Guid.Parse("DE6EFD89-2D62-4E63-98D2-AEA9E622B223")
						},
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=> x.Name == "TypeId"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);

			results.Any(x => x.Id == "6").ShouldBeTrue();
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}


		public void Dispose()
		{
			DocumentDbRepository.DeleteAllAsync<DocumentDbFoo3>(null).GetAwaiter().GetResult();
		}
	}
}
