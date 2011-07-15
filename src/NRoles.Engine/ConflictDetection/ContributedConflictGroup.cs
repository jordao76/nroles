using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  // a group for ONE contributed member to the target class from one or more role-composition members,
  // based on the WHOLE signature (that is, including the return type)
  public class ContributedConflictGroup : ConflictGroupBase { 
    ClassMember _supercedingMember;
    public bool HasConflict { get; internal set; }

    public IMemberDefinition ImplementedMember { get; set; }
    public bool DontImplement { get; set; }

    public IEnumerable<RoleCompositionMember> ResolveOverridingMembers() {
      // the overriding members are the members that are overriden by this group,
      // they include all the foreign group members that are not aliased (the aliased ones are overridden on other groups)
      // plus the overriding members of the resolved member

      return Members.
        Where(roleMember => roleMember.IsForeign && !roleMember.IsAliased).
        Concat(
          ResolvedMember != null ? 
            ResolvedMember.ResolveOverridingMembers() :
            new RoleCompositionMember[] { }).
        Distinct();
    }

    protected override bool SpecificMatches(RoleCompositionMember member) {
      // TODO: other things that don't match anything: constructors?
      return MemberMatcher.IsMatch(Members[0].ResolveContextualDefinition(), member.ResolveContextualDefinition());
    }

    // marks the member as superceded in the target type
    public void MarkAsSuperceded(ClassMember supercedingMember) {
      _supercedingMember = supercedingMember;
    }

    public ClassMember Supercede {
      get { return _supercedingMember; }
    }

    public bool IsSuperceded {
      get { return _supercedingMember != null; }
    }

    public IMemberDefinition Placeholder { get; set; }

    public bool IsBaseMethod { get; internal set; }

    public bool ReuseSlot { get; set; }

    public override ConflictDetectionResult Process() {
      return new GroupConflictResolver().Process(this);
    }

    public RoleCompositionMember ResolvedMember { get; internal set; }

    public string ResolveRepresentation() {
      return Members[0].ResolveContextualDefinition().ToString();
    }

    public override string ToString() {
      var sb = new StringBuilder();
      sb.AppendFormat("[Group]{1}{2} -> {0}\n",
        ResolveRepresentation(), 
        HasConflict ? " [Conflict]" : "",
        IsSuperceded ?  " [Superceded]" : "");
      Members.ForEach(roleMember => 
        sb.AppendFormat("  {1}{0}\n", 
          roleMember,
          ResolvedMember == roleMember ? "[Resolved] " : ""
      ));
      sb.Length -= "\n".Length;
      return sb.ToString();
    }

  }

}
