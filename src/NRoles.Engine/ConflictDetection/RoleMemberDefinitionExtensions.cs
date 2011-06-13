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
      member.CustomAttributes.Add(new CustomAttribute(
        new MethodReference(".ctor",
          module.Import(typeof(void))) {
          DeclaringType = module.Import(typeof(HideAttribute)),
          HasThis = true,
          ExplicitThis = false,
          CallingConvention = MethodCallingConvention.Default 
        }));
    }

    public static bool IsHidden(this IMemberDefinition member, ModuleDefinition module) {
      return member.IsMarkedWith<HideAttribute>();
    }

    public static void MarkAsGuarded(this IMemberDefinition member, ModuleDefinition module) {
      member.CustomAttributes.Add(new CustomAttribute(
        new MethodReference(".ctor",
          module.Import(typeof(void))) {
            DeclaringType = module.Import(typeof(GuardAttribute)),
            HasThis = true,
            ExplicitThis = false,
            CallingConvention = MethodCallingConvention.Default
          }));
    }

    public static bool IsGuarded(this IMemberDefinition member, ModuleDefinition module) {
      return member.IsMarkedWith<GuardAttribute>();
    }

    public static bool IsMarkedAsSupersede(this IMemberDefinition member, ModuleDefinition module) {
      if (!member.IsInRoleView()) return false;
      return member.IsMarkedWith<SupersedeAttribute>();
    }

    public static bool IsExcluded(this IMemberDefinition member, ModuleDefinition module) {
      if (!member.IsInRoleView()) return false;
      return member.IsMarkedWith<ExcludeAttribute>();
    }

    public static bool IsAliasing(this IMemberDefinition member, ModuleDefinition module) {
      string ignored;
      return member.IsAliasing(out ignored, module);
    }

    public static bool IsAliasing(this IMemberDefinition member, out string aliasing, ModuleDefinition module) {
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

    public static IMemberDefinition ResolveDefinitionInRole(this IMemberDefinition member, TypeReference type, ModuleDefinition module) {
      var finder = new MemberFinder(type);
      string aliasing;
      if (member.IsAliasing(out aliasing, module)) {
        return finder.FindMatchFor(member, aliasing);
      }
      else {
        return finder.FindMatchFor(member);
      }
    }

  }

}
