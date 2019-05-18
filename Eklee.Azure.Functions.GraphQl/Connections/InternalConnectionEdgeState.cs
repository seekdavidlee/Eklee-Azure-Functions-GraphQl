using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class InternalConnectionEdgeState
	{
		private readonly List<string> _list = new List<string>();

		private readonly Action<object> _entityAction;

		public InternalConnectionEdgeState(Action<object> entityAction)
		{
			_entityAction = entityAction;
		}

		public void InvokeAction(object item, string id, string type)
		{
			string key = $"{type}{id}";

			if (_list.IndexOf(key) == -1)
			{
				_list.Add(key);
				_entityAction?.Invoke(item);
			}
		}
	}
}
