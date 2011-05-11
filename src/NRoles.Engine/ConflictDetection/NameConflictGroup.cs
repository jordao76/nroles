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
  /// Naming conflicts can only happen if two or more members of different types share the same name.
  /// For example, the following members all have naming conflicts:
  /// <code>
  /// public class NamingConflicts {
  ///   public string Member { get; set; } // property
  ///   public event EventHandler Member; // event
  ///   private bool Member; // field
  ///   protected string Member(int param); // method
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
      // TODO: take aliasing, exclusion and hiding into account!
      var firstType = Members[0].Definition.GetType();
      return Members.Skip(1).Any(member => member.Definition.GetType() != firstType);
    }

  }

}
