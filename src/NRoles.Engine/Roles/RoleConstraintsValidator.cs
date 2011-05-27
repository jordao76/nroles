using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NRoles.Engine {

  public class RoleConstraintsValidator : IMutator {

    public IOperationResult Mutate(MutationParameters parameters) {
      parameters.Validate();
      return Check(parameters);
    }

    TypeDefinition _roleType;

    public RoleConstraintsValidator(TypeDefinition roleType) {
      if (roleType == null) throw new ArgumentNullException("roleType");
      _roleType = roleType;
    }

    public IOperationResult Check(MutationParameters parameters) {
      _roleType = parameters.SourceType;
      return Check(parameters.Context);
    }

    public IOperationResult Check(MutationContext context) {
      var result = new OperationResult();

      CheckNoClassesInheritFromRole(context, result);

      CheckNoInstancesAreCreatedForRole(context, result);

      CheckRoleDoesntComposeItself(result);

      CheckRoleDoesntImplementInterfacesExplicitly(result);

      /* TODO Checks:
        * a role can't be a struct
        * static members? right now it's being checked in the wrong class!
        * cannot have parameterized constructors - where is this being checked?
          * only a single parameterless constructor is supported
        * cannot have base classes (other than object), but can have base interfaces
        */

      return result;
    }

    private void CheckRoleDoesntComposeItself(OperationResult result) {
      if (_roleType.RetrieveDirectRoles().Any(role => role.Resolve() == _roleType)) {
        result.AddMessage(Error.RoleComposesItself(_roleType));
      }
    }

    private void CheckNoInstancesAreCreatedForRole(MutationContext context, OperationResult result) {
      var codeVisitor = new FindRoleInstantiation(_roleType, context);
      context.CodeVisitorsRegistry.Register(codeVisitor);
    }

    private void CheckNoClassesInheritFromRole(MutationContext context, OperationResult result) {
      if (_roleType.IsInterface) return;
      context.TypeVisitorsRegistry.Register(new FindRoleInheritance(_roleType, context));
    }

    private void CheckRoleDoesntImplementInterfacesExplicitly(OperationResult result) {
      var hasOverrides = _roleType.Methods.Any(m => m.HasOverrides);
      if (hasOverrides) {
        result.AddMessage(Error.RoleHasExplicitInterfaceImplementation(_roleType));
      }
    }

  }

}
