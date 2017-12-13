using Mono.Cecil.Cil;
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
      AssemblyMarkedWithDontMutate = 103,

      /// <summary>
      /// Occurs when a member marked as a placeholder in a composition doesn't
      /// match any role members. So there's nothing to replace it with.
      /// </summary>
      PlaceholderDoesntMatchAnyRoleMembers = 104

    }

    private Warning(Code number, string text, SequencePoint sequencePoint = null) : 
      base(MessageType.Warning, (int)number, text, sequencePoint: sequencePoint) { }

    internal static Warning PublicStaticMethodRelocation(object method, SequencePoint sequencePoint) {
      return new Warning(
        Code.PublicStaticMethodRelocation,
        $"The static method '{method}' will be relocated to the code class.",
        sequencePoint);
    }

    internal static Warning RoleMemberExcludedAgain(object roleView, object role, object member) {
      return new Warning(
        Code.RoleMemberExcludedAgain,
        $"The role member '{member}' of role '{role}' is being excluded multiple times (detected at role view '{roleView}').");
    }

    internal static Warning PublicStaticFieldRelocation(object field) {
      return new Warning(
        Code.PublicStaticFieldRelocation,
        $"The static field '{field}' will be relocated to the data class.");
    }

    // TODO: Move to Info class!
    internal static Warning AssemblyMarkedWithDontMutate(object assembly) {
      return new Warning(
        Code.AssemblyMarkedWithDontMutate,
        $"The assembly '{assembly}' is marked to not be mutated. Aborting mutation.");
    }

    internal static Warning PlaceholderDoesntMatchAnyRoleMembers(object member) {
      return new Warning(
        Code.PlaceholderDoesntMatchAnyRoleMembers,
        $"The member '{member}' is marked as a placeholder but there's no matching role member to replace it.");
    }
    
  }

}
