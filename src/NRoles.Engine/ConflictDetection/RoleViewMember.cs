using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public class RoleViewMember : RoleCompositionMember {

    public RoleViewMember(TypeReference role, IMemberDefinition member) :
      base(role, member) { }

    public override void Process() {
      var implementingMemberDefinition = ResolveImplementingMemberDefinition();
      if (this.HasError()) return;

      string aliasing;
      if (Definition.IsAliasing(out aliasing, Container.Module)) {
        // inform the immediate implementing member that it's been aliased
        if (Container[implementingMemberDefinition].IsAliased) {
          AddMessage(Error.RoleMemberAliasedAgain(Role, Container[implementingMemberDefinition].Role, Container[implementingMemberDefinition]));
        }
        Container[implementingMemberDefinition].MarkAsAliased();
      }

      if (Definition.IsExcluded(Container.Module)) {
        // inform the implementing member that it's been excluded
        if (Container[implementingMemberDefinition].IsExcluded) {
          AddMessage(Warning.RoleMemberExcludedAgain(Role, Container[implementingMemberDefinition].Role, Container[implementingMemberDefinition]));
        }
        Container[implementingMemberDefinition].MarkAsExcluded();
        // the role view member is also excluded
        MarkAsExcluded();
      }
      // TODO: Hidden?
    }

    public override IEnumerable<RoleCompositionMember> ResolveOverridingMembers() {
      // the overriding members are this role view member and the role member it refers to
      var implementingMemberDefinition = ResolveImplementingMemberDefinition();
      if (implementingMemberDefinition == null) throw new InvalidOperationException();
      return new RoleCompositionMember[] { this, Container[implementingMemberDefinition] };
    }

    public override RoleCompositionMember ResolveImplementingMember() {
      // the implementing member is the role member that this role view member refers to
      var implementingMemberDefinition = ResolveImplementingMemberDefinition();
      if (implementingMemberDefinition == null) return null;
      return Container[implementingMemberDefinition];
    }

    private IMemberDefinition _implementingMemberDefinition;
    private bool _implementingMemberDefinitionCached = false;
    private IMemberDefinition ResolveImplementingMemberDefinition() { // memoized
      if (_implementingMemberDefinitionCached) return _implementingMemberDefinition;
      _implementingMemberDefinitionCached = true;
      
      // look for the implementing member in the role view role
      var roleView = Definition.DeclaringType;

      // TODO: a role view must be an interface and must not implement any other interfaces than RoleView<TRole>, and exactly ONE of those, and it can't be generic!
      //   also, can't create a role view for a role view
      if (!roleView.IsInterface) {
        // NOTE: this check is at the wrong place. It will be called for each member in the role view and generate multiple duplicate messages.
        AddMessage(Error.RoleViewIsNotAnInterface(roleView));
        return null;
      }

      var allRolesForView = RetrieveAllRolesForView(roleView);

      if (allRolesForView.Count == 0) {
        throw new InvalidOperationException(); // TODO: AssertionException!
      }
      if (allRolesForView.Count > 1) { 
        AddMessage(Error.RoleViewWithMultipleRoles(roleView, allRolesForView));
        return null;
      }
      // allRolesForView contains a single role
      var roleForView = allRolesForView.Single();

      _implementingMemberDefinition =
        Definition.ResolveDefinitionInRole(roleForView, Container.Module);

      if (_implementingMemberDefinition == null) {
        // TODO: if it's being aliased, use the Aliasing name instead of the Definition name
        AddMessage(Error.RoleViewMemberNotFoundInRole(roleForView, Definition));
        return null;
      }

      return _implementingMemberDefinition;
    }

    public override bool IsAbstract {
      get { return false; }
    }

    private List<TypeReference> RetrieveAllRolesForView(TypeDefinition roleView) {
      var roleViewTypeDefinition = Container.Module.Import(typeof(RoleView<>)).Resolve();
      var allRolesForView = roleView.Interfaces
        .Where(interfaceTypeReference => interfaceTypeReference.Resolve() == roleViewTypeDefinition)
        .Select(roleViewTypeReference => ((GenericInstanceType)roleViewTypeReference).GenericArguments[0])
        .ToList();
      return allRolesForView;
    }

  }

}
