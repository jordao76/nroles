using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public class RoleMember : RoleCompositionMember {

    public RoleMember(TypeReference role, IMemberDefinition member) :
      base(role, member) { }

    public override void Process(MemberConflictResolver resolver) {
      resolver.Process(this);
    }

    public override bool IsForeign { get { return true; } }

    public override bool IsAbstract {
      get {
        // abstractedness is only applicable to method definitions
        return 
          (Definition is MethodDefinition) &&
          Role.Resolve().IsAbstract((MethodDefinition)Definition);
      }
    }

    public override RoleCompositionMember ResolveImplementingMember() {
      return this;
    }

    public override IEnumerable<RoleCompositionMember> ResolveOverridingMembers() {
      return new RoleCompositionMember[] { this };
    }

  }

}
