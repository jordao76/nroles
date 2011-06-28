using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Runtime.InteropServices;

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

      CheckRoleHasNoPInvokeMethods(result);

      CheckRoleHasNoPlaceholders(result);

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
      if (_roleType.IsAbstract) return;
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
        // Note: overrides can also be used in other languages that don't have the concept of explicit interface implementations.
        // TODO: This message is too specific for C#
        result.AddMessage(Error.RoleHasExplicitInterfaceImplementation(_roleType));
      }
    }

    private void CheckRoleHasNoPInvokeMethods(OperationResult result) {
      _roleType.Methods.Where(m => m.IsPInvokeImpl).
        ForEach(m => result.AddMessage(Error.RoleHasPInvokeMethod(m)));
    }

    private void CheckRoleHasNoPlaceholders(OperationResult result) {
      var members = new List<IMemberDefinition>();
      _roleType.Properties.ForEach(m => members.Add(m));
      _roleType.Events.ForEach(m => members.Add(m));
      _roleType.Methods.ForEach(m => members.Add(m));
      members.Where(m => m.IsMarkedAsPlaceholder()).
        ForEach(m => result.AddMessage(Error.RoleHasPlaceholder(m)));
    }

  }

}
