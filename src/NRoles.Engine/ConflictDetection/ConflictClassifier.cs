using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public interface IConflictClassifier {
    ModuleDefinition Module { get; set; }
    void Classify(IEnumerable<RoleCompositionMember> members);
    IEnumerable<IConflictGroup> Groups { get; }
  }

  public class ConflictClassifier<TConflictGroup> : IConflictClassifier
    where TConflictGroup : IConflictGroup, new()
  {
    List<IConflictGroup> _groups = new List<IConflictGroup>();
    public IEnumerable<IConflictGroup> Groups { get { return _groups; } }
    public ModuleDefinition Module { get; set; }

    public void Classify(IEnumerable<RoleCompositionMember> members) {
      if (members == null) throw new ArgumentNullException("members");
      members.ForEach(member => Classify(member));
    }

    private void Classify(RoleCompositionMember member) {
      if (member == null) throw new ArgumentNullException("member");
      var newGroup = false;
      var group = ResolveGroup(member);
      if (group == null) {
        group = new TConflictGroup() { Module = Module };
        newGroup = true;
      }
      if (group.Matches(member)) {
        group.AddMember(member);
        if (newGroup) {
          _groups.Add(group);
        }
      }
    }

    public IConflictGroup ResolveGroup(RoleCompositionMember member) {
      return _groups.SingleOrDefault(group => group.Matches(member));
    }
  }

}
