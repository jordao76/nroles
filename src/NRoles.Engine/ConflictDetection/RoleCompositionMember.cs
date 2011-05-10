using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public abstract class RoleCompositionMember : TypeMember, IMessageContainer {

    public TypeReference Role { get { return Type; } }

    protected RoleCompositionMember(TypeReference role, IMemberDefinition memberDefinition) :
      base(role, memberDefinition) { }

    public abstract void Process();

    // TODO: move this to the RoleViewMember?
    public RoleCompositionMemberContainer Container { get; internal set; }

    // the implementing member can be:
    //   1. the member itself
    //   2. if the member is excluded, the member that remains (in another role, or in the class itself)
    // ? 3. if the member is aliased, the alias for the member (can be many, but ONLY ONE will implement/override this member)
    //   4. if the member is overriden, the class member that overrode it
    public abstract RoleCompositionMember ResolveImplementingMember();

    public abstract IEnumerable<RoleCompositionMember> ResolveOverridingMembers();

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

    public abstract bool IsAbstract { get; }

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

    // TODO: role for message handling! eat your own dogfood!

    List<Message> _messages = new List<Message>();
    public IEnumerable<Message> Messages {
      get { return _messages; }
    }
    public void AddMessage(Message message) {
      _messages.Add(message);
    }

    #endregion

  }

}
