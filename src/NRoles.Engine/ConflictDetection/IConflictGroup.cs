using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Represents a member classification strategy, used to group 
  /// members that match a certain condition, normally indicating that 
  /// they're in conflict with each other.
  /// </summary>
  public interface IConflictGroup : IConflictDetector {

    /// <summary>
    /// The module currently being analyzed.
    /// </summary>
    ModuleDefinition Module { get; set; }

    /// <summary>
    /// Checks if a member matches the condition to be part of this conflict group.
    /// </summary>
    /// <param name="member">Member to check for a match.</param>
    /// <returns>If the member matches this group's condition.</returns>
    bool Matches(RoleCompositionMember member);

    /// <summary>
    /// Adds a member to this conflict group. It should fail if the member doesn't
    /// match the group's condition.
    /// </summary>
    /// <param name="member">Member to add to this group.</param>
    void AddMember(RoleCompositionMember member);

  }

}
