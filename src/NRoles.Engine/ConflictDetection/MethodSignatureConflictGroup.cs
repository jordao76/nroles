using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Groups methods that have matching parameters but different return types. 
  /// This is considered a signature conflict.
  /// </summary>
  public class MethodSignatureConflictGroup : ConflictGroupBase {

    protected override bool SpecificMatches(RoleCompositionMember member) {
      var definition = member.Definition;
      var method = definition as MethodDefinition;
      if (method == null) return false; // non-methods are not grouped
      var controlMethod = Members[0].Definition as MethodDefinition;
      if (controlMethod == null) return false;
      return MethodMatcher.IsSignatureMatch(method, controlMethod);
    }

    public override ConflictDetectionResult Process() {
      var result = new ConflictDetectionResult();

      if (HasConflict()) {
        result.AddMessage(
          Error.MethodsWithConflictingSignatures(FormatConflictingMembers()));
      }

      return result;
    }

    private bool HasConflict() {
      if (Members.Count <= 1) return false;
      // a signature conflict exists if at least 2 members differ in the return type
      var controlReturnType = ((MethodDefinition)Members[0].Definition).ReturnType;
      return Members.Any(member =>
        !TypeMatcher.IsMatch(
          controlReturnType, 
          ((MethodDefinition)member.Definition).ReturnType)
      );
    }

  }

}
