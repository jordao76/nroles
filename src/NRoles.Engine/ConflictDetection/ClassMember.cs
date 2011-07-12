using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Represents a member of a composition class (not from its roles).
  /// </summary>
  public class ClassMember : RoleCompositionMember {

    /// <summary>
    /// The class the member belongs to.
    /// </summary>
    public TypeReference Class { get { return Type; } }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="class">The member class.</param>
    /// <param name="definition">The member definition.</param>
    /// <param name="isInherited">If the member is inherited.</param>
    public ClassMember(TypeReference @class, IMemberDefinition definition, bool isInherited = false) :
      base(@class, definition)
    {
      IsInherited = isInherited;
    }

    /// <summary>
    /// Indicates if the represented member is inherited.
    /// </summary>
    /// <remarks>
    /// If it's an inherited member, the <see cref="Class"/> property will be the 
    /// parent class where the member is defined.
    /// </remarks>
    public bool IsInherited { get; private set; }

    /// <summary>
    /// Indicates if the member is a placeholder.
    /// A placeholder member is meant to be replaced.
    /// </summary>
    public bool IsPlaceholder {
      get { return Definition.IsPlaceholder(); }
    }

    /// <summary>
    /// Indicates if the member is abstract.
    /// </summary>
    public override bool IsAbstract {
      get {
        // abstractedness is only applicable to method definitions
        return
          Definition is MethodDefinition &&
          ((MethodDefinition)Definition).IsAbstract;
      }
    }

    /// <summary>
    /// A foreign member is a member that comes from outside the <see cref="Type"/>.
    /// <see cref="ClassMember"/>s are never foreign.
    /// </summary>
    public override bool IsForeign { 
      get { return false; } 
    }

    public override void Process() {
      var member = Definition;

      var memberGroup = Container.ResolveGroup(this);
      if (memberGroup.Members.Count == 1) { // no clash
        Tracer.Assert(memberGroup.Members.First() == this);
        if (IsPlaceholder) {
          AddMessage(Warning.PlaceholderDoesntMatchAnyRoleMembers(Definition));
        }
      }
      else {
        // if there's a match with other members, there's a conflict in the target type itself
        // it must be explicitly marked as [Supercede] to resolve the conflict,
        // or else a warning is created

        // TODO: the supercede can have any accessibility?
        // TODO: what if there's a clash and the supercede is NOT public?

        if (IsInherited) {
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
            memberGroup.MarkAsSuperceded(this);
          }

          return;
        }

        if (IsPlaceholder) {
          memberGroup.Placeholder = member;
          return;
        }
      }

      if (memberGroup == null) return;

      // TODO: DECIDE on the spelling: supersede vs supercede!!
      memberGroup.MarkAsSuperceded(this);
      if (!member.IsSupersede()) {
        // TODO: add a warning?
      }
    }

    public override RoleCompositionMember ResolveImplementingMember() {
      return this; // TODO: what if it's a placeholder?
    }

    public override IEnumerable<RoleCompositionMember> ResolveOverridingMembers() {
      return new RoleCompositionMember[] { };
    }

  }

}
