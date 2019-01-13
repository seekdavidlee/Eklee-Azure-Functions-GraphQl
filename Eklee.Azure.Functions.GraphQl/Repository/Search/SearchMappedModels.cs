﻿using System;
using System.Collections.Generic;
using System.Linq;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchMappedModel
	{
		public Type SearchModelType { get; set; }
		public Type ModelType { get; set; }
	}

	public interface ISearchMappedModels
	{
		void Map<TSearchModel, TModel>();
		bool TryGetMappedSearchType<TModel>(out Type mappedSearchType);
		object CreateInstanceFromMap<TModel>(TModel map);
	}

	public class SearchMappedModels : ISearchMappedModels
	{
		private readonly Dictionary<string, SearchMappedModel> _mappedTypes = new Dictionary<string, SearchMappedModel>();

		public void Map<TSearchModel, TModel>()
		{
			_mappedTypes.Add(typeof(TModel).Name, new SearchMappedModel
			{
				ModelType = typeof(TModel),
				SearchModelType = typeof(TSearchModel)
			});
		}

		public bool TryGetMappedSearchType<TModel>(out Type mappedSearchType)
		{
			if (_mappedTypes.ContainsKey(typeof(TModel).Name))
			{
				mappedSearchType = _mappedTypes[typeof(TModel).Name].SearchModelType;
				return true;
			}

			mappedSearchType = null;
			return false;
		}

		public object CreateInstanceFromMap<TModel>(TModel map)
		{
			var searchMappedModel = _mappedTypes[typeof(TModel).Name];
			var searchModelAccessor = TypeAccessor.Create(searchMappedModel.SearchModelType);
			var modelAccessor = TypeAccessor.Create(searchMappedModel.ModelType);

			var o = searchModelAccessor.CreateNew();

			var availableMemberNames = searchModelAccessor.GetMembers().Select(x => x.Name).ToList();

			modelAccessor.GetMembers().ToList().ForEach(m =>
			{
				if (availableMemberNames.Contains(m.Name))
					searchModelAccessor[o, m.Name] = modelAccessor[map, m.Name];
			});

			return o;
		}
	}
}