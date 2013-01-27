using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  static class RoleMemberDefinitionExtensions {

    // TODO: the module parameter proliferation is not ideal
    //   we solve this by using instance methods, not static method extensions!

    public static void MarkAsHidden(this IMemberDefinition member, ModuleDefinition module) {
      member.CustomAttributes.Add(module.Create<HideAttribute>());
    }

    public static bool IsHidden(this IMemberDefinition member, ModuleDefinition module) {
      return member.IsMarkedWith<HideAttribute>();
    }

    public static void MarkAsGuarded(this IMemberDefinition member, ModuleDefinition module) {
      member.CustomAttributes.Add(module.Create<GuardAttribute>());
    }

    public static bool IsGuarded(this IMemberDefinition member, ModuleDefinition module) {
      return member.IsMarkedWith<GuardAttribute>();
    }

    public static bool IsSupersede(this IMemberDefinition member) {
      return member.IsMarkedWith<SupercedeAttribute>();
    }

    public static bool IsMarkedAsPlaceholder(this IMemberDefinition member) {
      return member.IsMarkedWith<PlaceholderAttribute>();
    }

    public static bool IsPlaceholder(this IMemberDefinition member) {
      if (member.IsMarkedAsPlaceholder()) return true;
      var method = member as MethodDefinition;
      if (method == null) return false;
      // for an accessor method, see if its container (property or event) is a placeholder
      // TODO: won't this logic applies to other attributes? like Exclude, Aliasing, Hide, ...?
      if (method.IsPropertyAccessor()) {
        return method.ResolveContainerProperty().IsPlaceholder();
      }
      if (method.IsEventAccessor()) {
        return method.ResolveContainerEvent().IsPlaceholder();
      }
      return false;
    }

    public static bool IsExcluded(this IMemberDefinition member) {
      if (!member.IsInRoleView()) return false;
      return member.IsMarkedWith<ExcludeAttribute>();
    }

    public static bool IsAliasing(this IMemberDefinition member) {
      string ignored;
      return member.IsAliasing(out ignored);
    }

    public static bool IsAliasing(this IMemberDefinition member, out string aliasing) {
      aliasing = null;
      if (!member.IsInRoleView()) return false;
      if (member.IsMarkedWith<AliasingAttribute>()) {
        // NOTE: there COULD theoretically be multiple AliasingAttributes!
        aliasing = member.RetrieveAttributes<AliasingAttribute>().Single().ConstructorArguments[0].Value.ToString();
        // TODO: ERROR if aliasing == member.Name
        return true;
      }
      return false;
    }

    public static bool IsInRoleView(this IMemberDefinition member) {
      var roleType = member.DeclaringType;
      return roleType.IsRoleView();
    }

    public static IMemberDefinition ResolveDefinitionInRole(this IMemberDefinition member, TypeReference role) {
      var finder = new MemberFinder(role);
      string aliasing;
      if (member.IsAliasing(out aliasing)) {
        return finder.FindMatchFor(member, aliasing);
      }
      else {
        return finder.FindMatchFor(member);
      }
    }

  }

}
