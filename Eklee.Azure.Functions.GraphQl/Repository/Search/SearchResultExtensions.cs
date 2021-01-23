﻿using Azure;
using Azure.Search.Documents.Models;
using FastMember;
using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Connections;
using System.Collections;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public static class SearchResultExtensions
	{
		public static void AddFacets(this SearchResult searchResult, Response<SearchResults<SearchDocument>> response)
		{
			if (response.Value.Facets != null)
			{
				searchResult.Aggregates = response.Value.Facets.Select(f =>

					 new SearchAggregateModel
					 {
						 FieldName = f.Key,
						 FieldAggregates = f.Value.Select(v => new FieldAggregateModel { Count = Convert.ToInt32(v.Count), Value = GetFacetStringValue(v) }).ToList()
					 }).ToList();
			}
		}

		private static string GetFacetStringValue(FacetResult result)
		{
			if (result.Value is string strVal)
				return strVal;

			if (result.Value is DateTimeOffset dtoffVal)
				return dtoffVal.ToString("yyyy-MM-dd");

			return result.Value.ToString();
		}

		public static void AddValues(this SearchResult searchResult, TypeAccessor typeAccessor, Response<SearchResults<SearchDocument>> response)
		{
			var members = typeAccessor.GetMembers();

			response.Value.GetResults().ToList().ForEach(r =>
			{
				var item = typeAccessor.CreateNew();
				foreach (var d in r.Document)
				{
					PopulateField(typeAccessor, item, members, d);
				}

				searchResult.Values.Add(new SearchResultModel
				{
					Score = r.Score ?? 0,
					Value = item
				});
			});
		}

		private static void PopulateField(
			TypeAccessor typeAccessor,
			object item,
			MemberSet members,
			KeyValuePair<string, object> d)
		{
			var field = members.Single(x => x.Name == d.Key);

			object value = d.Value;

			if (field.Type == typeof(Guid) && value is string strValue)
			{
				typeAccessor[item, d.Key] = Guid.Parse(strValue);
				return;
			}

			if (field.Type == typeof(decimal) && value is double dobValue)
			{
				typeAccessor[item, d.Key] = Convert.ToDecimal(dobValue);
				return;
			}

			if ((field.Type == typeof(DateTime) || field.Type == typeof(DateTimeOffset)) && value is DateTimeOffset dtmValue)
			{
				typeAccessor[item, d.Key] = dtmValue.DateTime;
				return;
			}

			if (field.IsList())
			{
				if (value is SearchDocument[] doc1s)
				{
					typeAccessor[item, d.Key] = Populate(field, doc1s);
					return;
				}

				return;
			}

			if (field.Type != typeof(string) && field.Type.IsClass)
			{
				var t = TypeAccessor.Create(field.Type);
				var newItem = t.CreateNew();

				foreach (var dItem in (SearchDocument)value)
				{
					PopulateField(t, newItem, t.GetMembers(), dItem);
				}

				typeAccessor[item, d.Key] = newItem;
				return;
			}

			typeAccessor[item, d.Key] = Convert.ChangeType(value, field.Type);
		}

		private static object Populate(Member member, SearchDocument[] docs)
		{
			var listTypeAccessor = TypeAccessor.Create(member.Type);

			var list = (IList)listTypeAccessor.CreateNew();

			var type = member.Type.GetGenericArguments()[0];

			var typeAccessor = TypeAccessor.Create(type);

			var members = typeAccessor.GetMembers();

			foreach (var doc in docs)
			{
				var item = typeAccessor.CreateNew();
				list.Add(item);

				foreach (var d in doc)
				{
					PopulateField(typeAccessor, item, members, d);
				}
			}
			return list;
		}
	}
}
