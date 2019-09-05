using FastMember;
using System;

namespace Eklee.Azure.Functions.GraphQl.Actions.AutoIdPatterns
{
	public class GuidAutoIdPattern : IAutoIdPattern
	{		
		public object Generate(object item, Member member)
		{
			if (member.Type != typeof(string))
			{
				throw new InvalidOperationException("Cannot generate Guid for non-string types!");
			}

			return Guid.NewGuid().ToString("N");
		}
	}
}
