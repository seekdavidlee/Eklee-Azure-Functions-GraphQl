using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDbWithPermissions
{
	public class TestDocumentDbWithPermissionsMutation : ObjectGraphType
	{
		private bool DefaultTenantAssertion(ClaimsPrincipal claimsPrincipal, AssertAction assertAction)
		{
			return claimsPrincipal.IsInRole("Eklee.User.TestToFail");
		}

		private bool DefaultAdminAssertion(ClaimsPrincipal claimsPrincipal, AssertAction assertAction)
		{
			return claimsPrincipal.IsInRole("Eklee.Admin");
		}

		public TestDocumentDbWithPermissionsMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutation";

			var key = configuration["DocumentDb:Key"];
			var url = configuration["DocumentDb:Url"];
			var requestUnits = Convert.ToInt32(configuration["DocumentDb:RequestUnits"]);
			const string db = "docDb";

			inputBuilderFactory.Create<TestPermissionModel1>(this)
				.AssertWithClaimsPrincipal(DefaultAdminAssertion)
				.ConfigureDocumentDb<TestPermissionModel1>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Category)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All TestPermissionModel1 have been removed." })
				.Build();

			inputBuilderFactory.Create<TestPermissionModel2>(this)
				.AssertWithClaimsPrincipal(DefaultTenantAssertion)
				.ConfigureDocumentDb<TestPermissionModel2>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Category)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All TestPermissionModel2 have been removed." })
				.Build();

			inputBuilderFactory.Create<TestPermissionModel3>(this)
				.AssertWithClaimsPrincipal(DefaultAdminAssertion)
				.ConfigureDocumentDb<TestPermissionModel3>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Category)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All TestPermissionModel3 have been removed." })
				.Build();
		}
	}
}