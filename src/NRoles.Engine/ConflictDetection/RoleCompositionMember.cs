using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public abstract class RoleCompositionMember : TypeMember, IMessageContainer, Does<RMessageContainer> {

    public TypeReference Role { get { return base.Type; } }

    protected RoleCompositionMember(TypeReference type, IMemberDefinition memberDefinition) :
      base(type, memberDefinition) { }

    public abstract void Process();

    // TODO: move this to the RoleViewMember?
    public RoleCompositionMemberContainer Container { get; internal set; }

    // the implementing member can be:
    //   1. the member itself
    //   2. if the member is excluded, the member that remains (in another role, or in the class itself)
    // ? 3. if the member is aliased, the alias for the member (can be many, but ONLY ONE will implement/override this member)
    //   4. if the member is superceded, the class member that superceded it
    public abstract RoleCompositionMember ResolveImplementingMember();

    public abstract IEnumerable<RoleCompositionMember> ResolveOverridingMembers();

    // TODO: push these properties down the inheritance path?

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

    public extern IEnumerable<Message> Messages { [Placeholder] get; }
    [Placeholder] public extern void AddMessage(Message message);

    #endregion

  }

}
