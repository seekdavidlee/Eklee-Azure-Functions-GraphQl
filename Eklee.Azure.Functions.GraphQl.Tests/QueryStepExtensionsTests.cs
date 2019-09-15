using FastMember;
using Shouldly;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public class QueryStepTest
	{
		[Key]
		[Description("test")]
		public string Id { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class QueryStepExtensionsTests
	{
		[Fact]
		public void CanCloneQueryStep()
		{
			var queryStep = new QueryStep();
			bool isContextCalled = false;
			queryStep.ContextAction = (a) => { isContextCalled = true; };
			queryStep.StepMapper = (m) =>
			{
				return new List<object> { "a", "b" };
			};

			queryStep.Items = new Dictionary<string, object>();
			queryStep.Items.Add("foo", "bar");

			var type = typeof(QueryStepTest);
			var typeAccessor = TypeAccessor.Create(type);

			queryStep.QueryParameters.Add(new QueryParameter
			{
				Mapper = (m) => new List<object> { "a", "b" },
				ContextValue = new ContextValue
				{
					Comparison = Comparisons.Equal,
					Values = new List<object>
					{
						"value1","value2","value3"
					}
				},
				MemberModel = new ModelMember(type, typeAccessor, typeAccessor.GetMembers().Single(), false),
				Rule = new ContextValueSetRule
				{
					DisableSetSelectValues = false
				}
			});

			var clone = queryStep.CloneQueryStep();

			clone.ContextAction(null);

			isContextCalled.ShouldBe(true);

			var cem = clone.StepMapper(null);
			cem.Count.ShouldBe(2);
			cem[0].ShouldBe("a");
			cem[1].ShouldBe("b");

			clone.Items.Count.ShouldBe(1);
			clone.Items["foo"].ShouldBe("bar");

			clone.QueryParameters.Count.ShouldBe(1);

			var mapped = clone.QueryParameters[0].Mapper(null);
			mapped.Count.ShouldBe(2);
			mapped[0].ShouldBe("a");
			mapped[1].ShouldBe("b");

			clone.QueryParameters[0].Rule.ShouldNotBeNull();
			clone.QueryParameters[0].Rule.DisableSetSelectValues.ShouldBe(false);

			clone.QueryParameters[0].ContextValue.Comparison.ShouldBe(Comparisons.Equal);
			clone.QueryParameters[0].ContextValue.Values.Count.ShouldBe(3);
			clone.QueryParameters[0].ContextValue.Values[0].ShouldBe("value1");
			clone.QueryParameters[0].ContextValue.Values[1].ShouldBe("value2");
			clone.QueryParameters[0].ContextValue.Values[2].ShouldBe("value3");

			clone.QueryParameters[0].MemberModel.Name.ShouldBe("id");
			clone.QueryParameters[0].MemberModel.Description.ShouldBe("test");
		}
	}
}
