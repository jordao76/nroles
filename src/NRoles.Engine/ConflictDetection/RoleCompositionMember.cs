using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public abstract class RoleCompositionMember : TypeMember, IMessageContainer, Does<RMessageContainer> {

    [Obsolete("Use Type")]
    public TypeReference Role { get { return base.Type; } }

    protected RoleCompositionMember(TypeReference type, IMemberDefinition memberDefinition) :
      base(type, memberDefinition) { }

    public RoleCompositionMemberContainer Container { get; internal set; }

    /// <summary>
    /// Indicates if the member is abstract.
    /// </summary>
    public abstract bool IsAbstract { get; }

    /// <summary>
    /// A foreign member is a member that comes from outside the <see cref="Type"/>.
    /// </summary>
    public abstract bool IsForeign { get; }

    public abstract void Process();

    /// <summary>
    /// The implementing member is the member that "implements" this role/composition member.
    /// </summary>
    /// <remarks>
    /// The implementing member can be (but might not be limited to):
    ///   1. the member itself
    ///   2. if the member is excluded, the member that remains (in another role, or in the composition)
    ///   3. if the member is superceded, the class member that superceded it
    /// </remarks>
    /// <returns>Implementing member for this member.</returns>
    public abstract RoleCompositionMember ResolveImplementingMember();

    /// <summary>
    /// The inverse of the implementing member, the overriding members are all
    /// members that this member overrides. They all have this member as their 
    /// implementing member.
    /// </summary>
    /// <returns>Members that this member overrides.</returns>
    public abstract IEnumerable<RoleCompositionMember> ResolveOverridingMembers();

    // TODO: push these properties down the inheritance path? Or to a separate strategy object?

    // TODO: IsHidden?

    bool _isExcluded = false;
    public bool IsExcluded {
      get { return _isExcluded; }
    }
    public void MarkAsExcluded() {
      _isExcluded = true;
    }

    bool _isAliased = false;
    public bool IsAliased {
      get { return _isAliased; }
    }
    public void MarkAsAliased() {
      _isAliased = true;
    }

    public override string ToString() {
      var implementingMember = ResolveImplementingMember();
      return string.Format(
        "{1}{2}{3}{0}{4}", 
          Definition, 
          IsAliased ? "[Aliased] " : "",
          IsExcluded ? "[Excluded] " : "",
          IsAbstract ? "[Abstract] " : "",
          implementingMember == null ? " -> CAN'T RESOLVE" : (implementingMember.Definition != Definition ? (" -> " + implementingMember.Definition) : "")
        );
    }

    #region Messages

    public extern IEnumerable<Message> Messages { [Placeholder] get; }
    [Placeholder] public extern void AddMessage(Message message);

    #endregion

  }

}
