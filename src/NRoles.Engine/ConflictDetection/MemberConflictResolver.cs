using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {
  
  public class MemberConflictResolver : IMessageContainer, Does<RMessageContainer> {
    public RoleCompositionMemberContainer Container { get; set; }

    public void Process(ClassMember classMember) {
      var member = classMember.Definition;

      var memberGroup = Container.ResolveGroup(classMember);
      if (memberGroup.Members.Count == 1) { // no clash
        Tracer.Assert(memberGroup.Members.First() == classMember);
        if (classMember.IsPlaceholder) {
          AddMessage(Warning.PlaceholderDoesntMatchAnyRoleMembers(classMember.Definition));
        }
      }
      else {
        // if there's a match with other members, there's a conflict in the target type itself
        // it must be explicitly marked as [Supercede] to resolve the conflict,
        // or else a warning is created

        // TODO: the supercede can have any accessibility?
        // TODO: what if there's a clash and the supercede is NOT public?

        if (classMember.IsInherited) {
          // role members supercede base class members. Composition wins over inheritance!
          var method = member as MethodDefinition;
          if (method != null && method.IsVirtual && !method.IsFinal) {
            // reuses the virtual slot from the base class virtual method
            memberGroup.ReuseSlot = true;
          }

          // if all members in the group are abstract, supercede with the inherited member
          // TODO: what if the inherited member is also abstract?
          // TODO: it's very strange to have to look at the message to decide!
          var messages = memberGroup.Process().Messages;
          if (messages.Count() == 1 && messages.First().Number == (int)Error.Code.DoesNotImplementAbstractRoleMember) {
            // TODO: issue an info message that the role method is being silently superceded?
            memberGroup.MarkAsSuperceded(classMember);
          }

          return;
        }

        if (classMember.IsPlaceholder) {
          memberGroup.Placeholder = member;
          return;
        }
      }

      if (memberGroup == null) return;

      // TODO: DECIDE on the spelling: supersede vs supercede!!
      memberGroup.MarkAsSuperceded(classMember);
      if (!member.IsSupersede()) {
        // TODO: add a warning?
      }
    }

    public void Process(RoleViewMember roleViewMember) {
      var implementingMember = roleViewMember.ResolveImplementingMember();
      if (roleViewMember.HasError()) return;

      string aliasing;
      if (roleViewMember.Definition.IsAliasing(out aliasing)) {
        // inform the immediate implementing member that it's been aliased
        if (implementingMember.IsAliased) {
          AddMessage(Error.RoleMemberAliasedAgain(roleViewMember.Role, implementingMember.Role, implementingMember));
        }
        implementingMember.MarkAsAliased();
      }

      if (roleViewMember.Definition.IsExcluded()) {
        // inform the implementing member that it's been excluded
        if (implementingMember.IsExcluded) {
          AddMessage(Warning.RoleMemberExcludedAgain(roleViewMember.Role, implementingMember.Role, implementingMember));
        }
        implementingMember.MarkAsExcluded();
        // the role view member is also excluded
        roleViewMember.MarkAsExcluded();
      }
      // TODO: Hidden?
    }

    public void Process(RoleMember roleMember) {
      // no-op
    }
     
    public extern IEnumerable<Message> Messages { [Placeholder] get; }
    [Placeholder] public extern void AddMessage(Message message);
  }

}
