using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Groups methods that have the same name and matching parameter types.
  /// Detects signature conflicts by looking at return types in the group,
  /// checking if they are different.
  /// </summary>
  public class MethodSignatureConflictGroup : ConflictGroupBase {

    protected override bool MatchesEmptyGroup(RoleCompositionMember member) {
      return member.Definition is MethodDefinition;
    }

    protected override bool SpecificMatches(RoleCompositionMember member) {
      if (!(member.Definition is MethodDefinition)) return false; // non-methods are not grouped
      var controlMethod = Members[0];
      return MethodMatcher.IsSignatureMatch(
        (MethodDefinition)member.ResolveContextualDefinition(),
        (MethodDefinition)controlMethod.ResolveContextualDefinition());
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
