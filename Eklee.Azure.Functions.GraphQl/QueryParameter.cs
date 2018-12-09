using FastMember;

namespace Eklee.Azure.Functions.GraphQl
{
    public class QueryParameter
    {
        public TypeAccessor MemberParent { get; set; }
        public Member Member { get; set; }
        public ContextValue ContextValue { get; set; }
        public bool IsOptional { get; set; }

        public bool ValueEquals(object target)
        {
            return (IsOptional && ContextValue.IsNotSet) || MemberParent[target, Member.Name].Equals(ContextValue.Value);
        }
    }
}