using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  /// <summary>
  /// Represents a warning message.
  /// </summary>
  public sealed class Warning : Message {

    /// <summary>Warning codes.</summary>
    public enum Code {

      /// <summary>
      /// Occurs when a role has a public static method, which is relocated
      /// to the role code class.
      /// </summary>
      PublicStaticMethodRelocation = 100,

      /// <summary>
      /// Occurs when a role member is excluded from a composition more than one.
      /// </summary>
      RoleMemberExcludedAgain = 101,
    
      /// <summary>
      /// Occurs when a role has a public static field, which is relocated
      /// to the role data class.
      /// </summary>
      PublicStaticFieldRelocation = 102,

      /// <summary>
      /// Occurs when the assembly is marked with the DontMutate attribute, 
      /// which aborts the mutation process.
      /// </summary>
      AssemblyMarkedWithDontMutate = 103

    }

    private Warning(Code number, string text) : base(MessageType.Warning, (int)number, text) { }
    private Warning(Code number, string text, params object[] parameters) :
      this(number, string.Format(text, parameters)) { }

    internal static Warning PublicStaticMethodRelocation(object method) {
      return new Warning(
        Code.PublicStaticMethodRelocation,
        "The static method '{0}' will be relocated to the code class.", method);
    }

    internal static Warning RoleMemberExcludedAgain(object roleView, object role, object member) {
      return new Warning(
        Code.RoleMemberExcludedAgain,
        "The role member '{0}' of role '{1}' is being excluded multiple times (detected at role view '{2}').",
        member, role, roleView);
    }

    internal static Warning PublicStaticFieldRelocation(object field) {
      return new Warning(
        Code.PublicStaticFieldRelocation,
        "The static field '{0}' will be relocated to the data class.", field);
    }

    // TODO: Move to Info class!
    internal static Warning AssemblyMarkedWithDontMutate(object assembly) {
      return new Warning(
        Code.AssemblyMarkedWithDontMutate,
        "The assembly '{0}' is marked to not be mutated. Aborting mutation.", assembly);
    }
    
  }

}
