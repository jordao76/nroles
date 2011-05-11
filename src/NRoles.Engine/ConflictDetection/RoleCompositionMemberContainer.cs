using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  // TODO: refactor this class!!
  //   merge with the other container class?
  [Obsolete]
  public class RoleCompositionMemberContainer : IMessageContainer {

    HashSet<RoleCompositionMember> _members = new HashSet<RoleCompositionMember>();
    ConflictClassifier<ContributedConflictGroup> _classifier;

    public ModuleDefinition Module { get; private set; }
    public TypeDefinition TargetType { get; private set; }

    public RoleCompositionMemberContainer(TypeDefinition targetType) {
      if (targetType == null) throw new ArgumentNullException();
      TargetType = targetType;
      Module = targetType.Module;
      AddTargetTypeMembers();
    }

    public void AddTargetTypeMembers() {
      var container = new ClassMemberContainer(TargetType);
      container.Members.ForEach(member => AddMember(member));
    }

    public void AddMember(RoleCompositionMember typeMember) {
      if (typeMember == null) throw new ArgumentNullException("member");
      _members.Add(typeMember);
      typeMember.Container = this;
    }

    public void Process() {
      ValidateMembers();
      Group();
      ResolveConflicts();
    }

    private void ValidateMembers() {
      // runs validating classifications

      ProcessGroups(Classify<MethodSignatureConflictGroup>());

      ProcessGroups(Classify<NameConflictGroup>());

      // TODO: other validations
      //   generic clashes?
    }

    private ConflictClassifier<TConflictGroup> Classify<TConflictGroup>() where TConflictGroup : IConflictGroup, new() {
      var classifier = new ConflictClassifier<TConflictGroup>();
      classifier.TargetType = TargetType;
      classifier.Classify(_members);
      return classifier;
    }

    private void ProcessGroups<TConflictGroup>(ConflictClassifier<TConflictGroup> classifier) where TConflictGroup : IConflictGroup, new() {
      classifier.Groups.ForEach(group => this.Slurp(group.Process()));
      classifier.TraceGroups();
    }

    private void Group() {
      _classifier = Classify<ContributedConflictGroup>();
    }

    private void ResolveConflicts() {
      ProcessMembers();
      ProcessGroups();
    }

    private void ProcessMembers() {
      _members.ForEach(member => member.Process()); // TODO: slurp?
    }

    private void ProcessGroups() {
      ProcessGroups(_classifier);
    }

    public RoleCompositionMember ResolveMember(IMemberDefinition memberDefinition) {
      return _members.Single(roleMember => roleMember.Definition == memberDefinition);
    }

    public ContributedConflictGroup ResolveGroup(TypeReference type, IMemberDefinition memberDefinition) {
      var member = new RoleMember(type, memberDefinition);
      return ResolveGroup(member);
    }

    public ContributedConflictGroup ResolveGroup(RoleCompositionMember member) {
      return _classifier.ResolveGroup(member);
    }

    public IEnumerable<ContributedConflictGroup> RetrieveMemberGroups() {
      return _classifier.Groups;
    }

    List<Message> _messages = new List<Message>();
    public IEnumerable<Message> Messages {
      get {
        return _messages.Concat(_members.SelectMany(m => m.Messages));
      }
    }
    public void AddMessage(Message message) {
      _messages.Add(message);
    }

  }

}
