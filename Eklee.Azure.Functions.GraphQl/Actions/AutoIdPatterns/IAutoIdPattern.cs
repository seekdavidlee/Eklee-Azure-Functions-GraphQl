using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Actions.AutoIdPatterns
{
	public interface IAutoIdPattern
	{
		object Generate(object item, Member member);
	}
}
