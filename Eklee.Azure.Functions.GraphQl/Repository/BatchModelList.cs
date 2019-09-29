using FastMember;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class BatchModelList
	{
		private readonly Dictionary<string, object> _list = new Dictionary<string, object>();
		private readonly TypeAccessor _typeAccessor;

		/// <summary>
		/// Constructor for BatchModelList.
		/// </summary>
		/// <param name="type">Type.</param>
		public BatchModelList(Type type)
		{
			_typeAccessor = TypeAccessor.Create(type);
		}

		/// <summary>
		/// Adds a model to the internal list which will check for duplicates.
		/// </summary>
		/// <param name="model"></param>
		public void Add(object model)
		{
			var key = _typeAccessor.GetModelKey(model);

			if (!_list.ContainsKey(key))
			{
				_list.Add(key, model);
			}
		}

		/// <summary>
		/// Gets a list of non-duplicated models.
		/// </summary>
		public List<object> Items
		{
			get
			{
				return _list.Values.ToList();
			}
		}
	}
}
