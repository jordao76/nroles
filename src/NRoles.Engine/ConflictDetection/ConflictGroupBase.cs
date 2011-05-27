using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Base class for conflict grouping strategies.
  /// </summary>
  public abstract class ConflictGroupBase : IConflictGroup {
    
    /// <summary>
    /// The module currently being analyzed.
    /// </summary>
    public ModuleDefinition Module { get; set; }

    /// <summary>
    /// The members in this group.
    /// </summary>
    public IList<RoleCompositionMember> Members { get; private set; }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    protected ConflictGroupBase() {
      Members = new List<RoleCompositionMember>();
    }

    /// <summary>
    /// Do the conflict detection.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public abstract ConflictDetectionResult Process();

    /// <summary>
    /// Checks if a member matches the condition to be part of this conflict group.
    /// </summary>
    /// <remarks>
    /// The member will always match a group that it's already part of.
    /// For an empty group, the method <see cref="MatchesEmptyGroup"/> is called.
    /// The member won't match if it's a hidden member. Hidden members can't conflict with other members.
    /// The method <see cref="SpecificMatches"/> is called to check this group's condition.
    /// </remarks>
    /// <param name="member">Member to check for a match.</param>
    /// <returns>If the member matches this group's condition.</returns>
    public bool Matches(RoleCompositionMember member) { 
      if (member == null) throw new ArgumentNullException("member");
      if (Members.Count == 0) return MatchesEmptyGroup(member);
      if (MemberMatcher.IsMatch(Members[0].ResolveContextualDefinition(), member.ResolveContextualDefinition())) return true;
      if (member.Definition.IsHidden(Module)) return false;
      return SpecificMatches(member);
    }

    /// <summary>
    /// Called to decide if a member can be part of an empty group, that is, 
    /// without considering other members. This is the initial condition for a
    /// member to be part of this group. By default returns <c>true</c>.
    /// </summary>
    /// <param name="member">Member to check.</param>
    /// <returns>If the member should be part of an empty group.</returns>
    protected virtual bool MatchesEmptyGroup(RoleCompositionMember member) {
      return true;
    }

    /// <summary>
    /// Runs the specific condition logic for this conflict group,
    /// to be defined by subclasses.
    /// </summary>
    /// <param name="member">Member to match.</param>
    /// <returns>If the member matches this group's condition.</returns>
    protected abstract bool SpecificMatches(RoleCompositionMember member);

    /// <summary>
    /// Adds a member to this conflict group. It will fail if the member doesn't
    /// match the group's condition.
    /// </summary>
    /// <param name="member">Member to add to this group.</param>
    public void AddMember(RoleCompositionMember member) {
      if (member == null) throw new ArgumentNullException("member");
      if (Members.Contains(member)) return;
      // TODO: rename matches to accepts?
      if (!Matches(member)) throw new ArgumentException("member doesn't match group");
      Members.Add(member);
    }

    /// <summary>
    /// Formats the members in the group as a string. For error reporting.
    /// </summary>
    /// <returns>The members in the group formatted as a string.</returns>
    protected string FormatConflictingMembers() {
      return " - '" +
        string.Join("', '", Members.Select(member => member.ToString()).ToArray()) + "'";
    }

    /// <summary>
    /// Returns a string representation of this conflict group.
    /// </summary>
    /// <returns>A string representation of this conflict group.</returns>
    public override string ToString() {
      var sb = new StringBuilder();
      sb.AppendFormat("[Group]\n");
      Members.ForEach(member => sb.AppendFormat("  {0}\n", member.Definition));
      sb.Length -= "\n".Length;
      return sb.ToString();
    }
  }

}
