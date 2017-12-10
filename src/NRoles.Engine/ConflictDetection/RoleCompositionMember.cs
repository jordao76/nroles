using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  // TODO: try to get rid of this type
  public abstract class RoleCompositionMember : TypeMember, IMessageContainer {

    [Obsolete("Use Type")]
    public TypeReference Role { get { return base.Type; } }

    protected RoleCompositionMember(TypeReference type, IMemberDefinition memberDefinition) :
      base(type, memberDefinition) { }

    /// <summary>
    /// Indicates if the member is abstract.
    /// </summary>
    public abstract bool IsAbstract { get; }

    /// <summary>
    /// A foreign member is a member that comes from outside the <see cref="Type"/>.
    /// </summary>
    public abstract bool IsForeign { get; }

    public RoleCompositionMemberContainer Container { get; internal set; }

    // TODO: return a result and make this type not a message container?
    public void Process() {
      // TODO: clear messages
      var resolver = new MemberConflictResolver();
      resolver.Container = Container;
      Process(resolver);
      this.Slurp(resolver);
    }
    
    public abstract void Process(MemberConflictResolver resolver);

    #region Member Markings

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

    // TODO: IsHidden?

    public virtual bool IsExcluded { get; private set; }
    public virtual void MarkAsExcluded() { IsExcluded = true; }

    public virtual bool IsAliased { get; private set; }
    public virtual void MarkAsAliased() { IsAliased = true; }

    public override string ToString() {
      var implementingMember = ResolveImplementingMember();
      return string.Format(
        "{2}{3}{4}{1}::{0}{5}",
          ResolveContextualDefinition(),
          Type,
          IsAliased ? "[Aliased] " : "",
          IsExcluded ? "[Excluded] " : "",
          IsAbstract ? "[Abstract] " : "",
          implementingMember == null ? 
            " -> CAN'T RESOLVE" : 
            (implementingMember.Definition != Definition ? 
              (" -> " + implementingMember.ResolveContextualDefinition()) : "")
        );
    }
    #endregion

    #region Messages

    private List<Message> _messages = new List<Message>();
    public IEnumerable<Message> Messages {
      get { return _messages; }
    }
    public void AddMessage(Message message) {
      if (message == null) throw new ArgumentNullException("message");
      _messages.Add(message);
    }

    #endregion

  }

}
