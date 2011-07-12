using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.Text.RegularExpressions;

namespace NRoles.Engine {
  
  static class RoleUtils {

    public static bool IsRole(this TypeDefinition self) {
      return self.Interfaces.Cast<TypeReference>().Any(
        type => type.Resolve().FullName == typeof(Role).FullName);
    }

    public static bool IsRoleView(this TypeReference role) {
      var interfaces = role.Resolve().Interfaces;
      if (interfaces != null && interfaces.Count > 0) {
        return interfaces.Any(IsRoleViewInterface);
        }
      return false;
    }

    public static bool IsRoleViewInterface(this TypeReference interfaceReference) {
      return interfaceReference.Resolve().FullName == typeof(RoleView<>).FullName;
    }

    #region Retrieve roles

    public static IEnumerable<TypeReference> RetrieveDirectRoles(this TypeReference self) {
      return
        from @interface in self.Resolve().Interfaces
        where @interface.DoesRole()
        select @interface.RetrieveRole();
    }
    
    public static IEnumerable<TypeReference> RetrieveRoles(this TypeReference self) {
      var roles = new Dictionary<TypeReference, int>();
      self.RetrieveRoles(roles);
      return roles.Keys.OrderByDescending(role => roles[role]); // sort roles from base to derived
    }

    private static void RetrieveRoles(this TypeReference self, Dictionary<TypeReference, int> roleCollector, int sortOrder = 1) {
      // Note: the roles from the base types are already implemented, so there's no need to reimplement them
      var directRoles = self.RetrieveDirectRoles();
      // get the roles from the direct roles
      directRoles.ForEach(role => {
        if (!(self is TypeDefinition)) {
          role = new MemberResolver(self).ResolveConstituentType(role);
        }
        var roleInCollector = roleCollector.Keys.SingleOrDefault(type => TypeMatcher.IsMatch(type, role));
        if (roleInCollector != null) {
          if (roleCollector[roleInCollector] < sortOrder) {
            roleCollector[roleInCollector] = sortOrder;
            role.RetrieveRoles(roleCollector, sortOrder + 1); // have to recurse to adjust the parents' sort order
          }
        }
        else {
          roleCollector.Add(role, sortOrder);
          role.RetrieveRoles(roleCollector, sortOrder + 1);
        }
      });
    }

    public static bool DoesRole(this TypeReference declaredInterface) {
      return declaredInterface.Resolve().FullName == typeof(Does<>).FullName;
    }

    private static TypeReference RetrieveRole(this TypeReference doesReference) {
      var does = (GenericInstanceType)doesReference;
      return does.GenericArguments[0];
    }

    #endregion

    public static bool IsBaseMethod(this IMemberDefinition self) {
      return self is MethodDefinition &&
        NameProvider.IsVirtualBaseMethod(self.Name);
    }

    // code class

    public static TypeDefinition ResolveCodeClass(this TypeReference roleReference) {
      // TODO: create separate strategies!
      var role = roleReference.Resolve();
      var codeClassName = role.FullName + "/" + NameProvider.GetCodeClassName(role.Name);
      var codeClass = role.NestedTypes.SingleOrDefault(nt => nt.FullName == codeClassName);
      if (codeClass == null) {
        // NOTE, TODO: the alternative strategy is only used for testing!
        codeClassName = new Regex(role.Name + "$").Replace(role.FullName, "") + NameProvider.GetAlternativeCodeClassName(role.Name);
        codeClass = role.Module.GetType(codeClassName);
      }
      if (codeClass == null) throw new InvalidOperationException("No code class for role " + role); // TODO:assert
      return codeClass;
    }

    public static MethodDefinition ResolveCorrespondingMethod(this TypeDefinition role, MethodDefinition roleMethod) {
      var codeClass = role.ResolveCodeClass();
      if (codeClass == null) throw new InvalidOperationException();
      var correspondingMethod = new CorrespondingMethodFinder(codeClass).FindMatchFor(roleMethod);
      return correspondingMethod;
    }

    public static bool IsAbstract(this TypeDefinition role, MethodDefinition roleMethod) {
      // for the method to be considered abstract, it must not be present in the code class 
      //   and it must not be the state class getter
      if (IsRoleView(role)) throw new InvalidOperationException();
      return
        roleMethod.Name != NameProvider.GetStateClassPropertyGetterName(role.Name) &&
        role.ResolveCorrespondingMethod(roleMethod) == null;
    }

    // state class

    public static TypeDefinition ResolveStateClass(this TypeReference role) {
      var stateClassName = role.Resolve().FullName + "/" + NameProvider.GetStateClassName(role.Name);
      return role.Resolve().NestedTypes.Single(nt => nt.FullName == stateClassName);
    }

    public static MethodReference ResolveStateClassCtor(this TypeReference role) {
      var stateClassDefinition = ResolveStateClass(role);
      var ctorDefinition = stateClassDefinition.Methods.Where(m => m.IsConstructor).Single(); // TODO: get constructors extension method!
      return new MemberResolver(role).ResolveMethodReference(ctorDefinition);
    }

  }

}
