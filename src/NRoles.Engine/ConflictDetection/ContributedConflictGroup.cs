using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  // a group for ONE contributed member to the target class from one or more role members,
  // based on the WHOLE signature (that is, including the return type)
  // TODO: separate conflict detection from member resolution? create an IConflictResolver?
  public class ContributedConflictGroup : ConflictGroupBase { 
    TypeDefinition _targetType;
    ClassMember _supercedingMember;
    bool _hasConflict;

    // TODO: this does not belong here, create another class that associates a ConflictGroup with its implemented member
    public IMemberDefinition ImplementedMember { get; set; }
    public bool DontImplement { get; set; }

    public ContributedConflictGroup(TypeDefinition targetType) {
      if (targetType == null) throw new ArgumentNullException("targetType");
      _targetType = targetType;
      Module = _targetType.Module;
    }

    public IEnumerable<RoleCompositionMember> ResolveOverridingMembers() {
      // the overriding members are the members that are overriden by this group,
      // they include all the group members that are not aliased (the aliased ones are overridden on other groups)
      // plus the implementing member of the resolved member

      return Members.
        Where(roleMember => !roleMember.IsAliased).
        Concat(
          ResolvedMember != null ? 
            ResolvedMember.ResolveOverridingMembers() :
            Enumerable.Empty<RoleCompositionMember>()).
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

    public bool IsBaseMethod { get; private set; }

    public bool ReuseSlot { get; set; }

    public override ConflictDetectionResult Process() {
      ResolvedMember = null;
      _hasConflict = false;

      var result = new ConflictDetectionResult();

      if (IsSuperceded) return result; // TODO: process warnings? views here are superfluous since the member is overriden!

      // to solve the conflict, EXACTLY one non-abstract member must remain in the list

      // process excluded members
      var resolvedMembers = Members.Where(roleMember => !roleMember.IsExcluded).ToList();
      if (resolvedMembers.Count == 0) {
        result.AddMessage(Error.AllMembersExcluded(_targetType, ResolveRepresentation()));
        return result;
      }

      // process aliased members
      resolvedMembers = resolvedMembers.Where(roleMember => !roleMember.IsAliased).ToList();
      if (resolvedMembers.Count == 0) {
        // all members are aliased
        ResolvedMember = null;
        return result;
      }

      // process base methods
      if (resolvedMembers.All(roleMember => roleMember.Definition.IsBaseMethod())) {
        // all members are virtual base members, they'll be provided by the composing class
        IsBaseMethod = true;
        ResolvedMember = null;
        return result;
      }
      if (resolvedMembers.Any(roleMember => roleMember.Definition.IsBaseMethod())) {
        throw new InvalidOperationException("Base methods cannot be provided by roles");
      }

      if (resolvedMembers.All(roleMember => roleMember.IsAbstract)) {
        if (!_targetType.IsRole()) {
          result.AddMessage(Error.DoesNotImplementAbstractRoleMember(_targetType, ResolveRepresentation()));
        }
        return result;
      }

      // process abstract members
      resolvedMembers = resolvedMembers.Where(roleMember => !roleMember.IsAbstract).ToList();
      if (resolvedMembers.Count > 1) {
        _hasConflict = true;
        result.AddMessage(Error.Conflict(_targetType, ResolveRepresentation(), resolvedMembers));
        return result;
      }

      ResolvedMember = resolvedMembers.Single();

      return result;
    }

    public RoleCompositionMember ResolvedMember { get; private set; }

    private string ResolveRepresentation() {
      return Members[0].ResolveContextualDefinition().ToString();
    }

    public override string ToString() {
      var sb = new StringBuilder();
      sb.AppendFormat("[Member Group]{1}{2} -> {0}\n",
        ResolveRepresentation(), 
        _hasConflict ? " [Conflict]" : "",
        IsSuperceded ?  " [Superceded]" : "");
      Members.ForEach(roleMember => 
        sb.AppendFormat("  {2}{0}::{1}\n", 
          roleMember.Type,
          roleMember.ResolveContextualDefinition(),
          ResolvedMember == roleMember ? "[Resolved] " : ""
      ));
      sb.Length -= "\n".Length;
      return sb.ToString();
    }

  }

}
