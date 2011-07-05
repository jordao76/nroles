using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public interface IConflictClassifier {
    void Classify(IEnumerable<RoleCompositionMember> members);
    IEnumerable<IConflictGroup> Groups { get; }
  }

  public class ConflictClassifier<TConflictGroup> : IConflictClassifier
    where TConflictGroup : IConflictGroup, new()
  {
    List<TConflictGroup> _groups = new List<TConflictGroup>();
    public IEnumerable<TConflictGroup> Groups { get { return _groups; } }
    public TypeDefinition TargetType { get; set; }

    public void Classify(IEnumerable<RoleCompositionMember> members) {
      if (members == null) throw new ArgumentNullException("members");
      members.ForEach(member => Classify(member));
    }

    private void Classify(RoleCompositionMember member) {
      if (member == null) throw new ArgumentNullException("member");
      var newGroup = false;
      var group = ResolveGroup(member);
      if (group == null) {
        group = new TConflictGroup() { TargetType = TargetType };
        newGroup = true;
      }
      if (group.Matches(member)) {
        group.AddMember(member);
        if (newGroup) {
          _groups.Add(group);
        }
      }
    }

    public TConflictGroup ResolveGroup(RoleCompositionMember member) {
      return _groups.SingleOrDefault(group => group.Matches(member));
    }

    void IConflictClassifier.Classify(IEnumerable<RoleCompositionMember> members) {
      throw new NotImplementedException();
    }

    IEnumerable<IConflictGroup> IConflictClassifier.Groups {
      get {
        foreach (var group in Groups) {
          yield return group;
        }
      }
    }

  }

}
