using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  public class GroupConflictResolver {

    public ConflictDetectionResult Process(ContributedConflictGroup group) {
      group.ResolvedMember = null;
      group.HasConflict = false;

      var result = new ConflictDetectionResult();

      if (group.IsSuperceded) {
        return result; // TODO: process warnings? views here are superfluous since the member is overriden!
      }

      // to solve the conflict, EXACTLY one non-abstract member must remain in the list

      // only consider foreign members
      var resolvedMembers = group.Members.Where(roleMember => roleMember.IsForeign).ToList();

      // process excluded members
      resolvedMembers = resolvedMembers.Where(roleMember => !roleMember.IsExcluded).ToList();
      if (resolvedMembers.Count == 0) {
        result.AddMessage(Error.AllMembersExcluded(group.TargetType, group.ResolveRepresentation()));
        return result;
      }

      // process aliased members
      resolvedMembers = resolvedMembers.Where(roleMember => !roleMember.IsAliased).ToList();
      if (resolvedMembers.Count == 0) {
        // all members are aliased
        group.ResolvedMember = null;
        return result;
      }

      // process base methods
      if (resolvedMembers.All(roleMember => roleMember.Definition.IsBaseMethod())) {
        // all members are virtual base members, they'll be provided by the composing class
        group.IsBaseMethod = true;
        group.ResolvedMember = null;
        return result;
      }
      Tracer.Assert(!resolvedMembers.Any(roleMember => roleMember.Definition.IsBaseMethod()), "Base methods cannot be provided by roles");

      if (resolvedMembers.All(roleMember => roleMember.IsAbstract)) {
        if (!group.TargetType.IsRole()) {
          result.AddMessage(Error.DoesNotImplementAbstractRoleMember(group.TargetType, group.ResolveRepresentation()));
        }
        return result;
      }

      // process abstract members
      resolvedMembers = resolvedMembers.Where(roleMember => !roleMember.IsAbstract).ToList();
      if (resolvedMembers.Count > 1) {
        group.HasConflict = true;
        result.AddMessage(Error.Conflict(group.TargetType, group.ResolveRepresentation(), resolvedMembers));
        return result;
      }

      group.ResolvedMember = resolvedMembers.Single();

      return result;
    }

  }

}
