using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Groups members based on naming conflicts.
  /// </summary>
  /// <remarks>
  /// Naming conflicts can only happen if two members that are not overloads share the same name.
  /// This is true for non-methods that share the same name with methods or with other non-methods.
  /// For example, the following members all have naming conflicts:
  /// <code>
  /// public class NamingConflicts {
  ///   public string Member { get; set; }
  ///   public event EventHandler Member;
  ///   private bool Member;
  ///   protected string Member(int param);
  /// }
  /// </code>
  /// </remarks>
  public class NameConflictGroup : ConflictGroupBase {

    protected override bool SpecificMatches(RoleCompositionMember member) {
      return member.Definition.Name == Members[0].Definition.Name;
    }

    public override ConflictDetectionResult Process() {
      var result = new ConflictDetectionResult();

      if (HasConflict()) {
        result.AddMessage(
          Error.MembersWithSameName(FormatConflictingMembers())
        );
      }

      return result;
    }

    private bool HasConflict() {
      if (Members.Count <= 1) return false;
      // a name conflict exists if at least one member is NOT a method
      // methods are accounted for in other classifiers
      // TODO: take aliasing and exclusion into account!
      return Members.Any(member => !(member.Definition is MethodDefinition));
    }

  }

}
