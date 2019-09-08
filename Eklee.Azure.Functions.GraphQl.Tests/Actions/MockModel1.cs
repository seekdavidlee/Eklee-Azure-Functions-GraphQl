using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Tests.Actions
{
	public class MockModel1
	{
		public string Id { get; set; }

		public List<MockModel2> Model2List { get; set; }
	}
}
