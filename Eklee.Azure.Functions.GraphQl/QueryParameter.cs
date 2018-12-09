namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryParameter
	{
		public ModelMember MemberModel { get; set; }
		public ContextValue ContextValue { get; set; }

		public bool ValueEquals(object target)
		{
			return (MemberModel.IsOptional && ContextValue.IsNotSet) ||
				   MemberModel.PathMemberValueEquals(target, ContextValue.Value);
		}
	}
}