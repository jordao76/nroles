using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  // TODO: refactor this class!!
  //   merge with the other container class?
  public class RoleCompositionMemberContainer : IMessageContainer {

    HashSet<RoleCompositionMember> _members = new HashSet<RoleCompositionMember>();
    IList<ContributedConflictGroup> _conflictGroups = new List<ContributedConflictGroup>();

    public ModuleDefinition Module { get; private set; }
    public TypeDefinition TargetType { get; private set; }

    public RoleCompositionMemberContainer(TypeDefinition targetType) {
      if (targetType == null) throw new ArgumentNullException();
      TargetType = targetType;
      Module = targetType.Module;
    }

    public void AddMember(RoleCompositionMember typeMember) {
      if (typeMember == null) throw new ArgumentNullException("member");
      _members.Add(typeMember);
      typeMember.Container = this;
    }

    public void Process() {
      ValidateMembers();
      ProcessMembers();
      Group();
      ProcessTargetTypeMembers();
      ProcessGroups();
    }

    private void ValidateMembers() {
      // runs validating classifications

      Classify<MethodSignatureConflictGroup>();

      Classify<NameConflictGroup>();

      // TODO: other validations
      //   generic clashes
    }

    private void Classify<TConflictGroup>() where TConflictGroup : IConflictGroup, new() {
      var classifier = new ConflictClassifier<TConflictGroup>();
      classifier.Module = TargetType.Module;
      classifier.Classify(_members);
      classifier.Groups.ForEach(group => this.Slurp(group.Process()));
      TraceGroups(classifier);
    }

    private void ProcessMembers() {
      // TODO: remove the Process method from the RoleViewMember
      _members.ForEach(member => member.Process());
    }

    private void Group() {
      _members.ForEach(member => GroupMember(member));
    }

    private void ProcessGroups() {
      _conflictGroups.ForEach(group => this.Slurp(group.Process()));
    }

    // TODO: this code is now in ConflictClassifier
    private void GroupMember(RoleCompositionMember memberToGroup) {
      var memberGroup = ResolveGroup(memberToGroup.Definition);
      if (memberGroup == null) {
        memberGroup = new ContributedConflictGroup(this);
        _conflictGroups.Add(memberGroup);
      }
      memberGroup.AddMember(memberToGroup);
    }

    private void ProcessTargetTypeMembers() {
      var container = new ClassMemberContainer(TargetType);
      container.Members.ForEach(member => ProcessTargetTypeMember(member));
    }

    private void ProcessTargetTypeMember(ClassMember classMember) {
      var member = classMember.Definition;

      var memberGroup = ResolveGroup(member);
      if (memberGroup == null) return; // no clash

      // if there's a match, there's a conflict in the target type itself
      // it must be explicitly marked as [Supercede] to resolve the conflict,
      // or else a warning is created

      // TODO: the supercede must be public or protected!
      // TODO: what if there's a clash and the supercede is NOT public?

      if (classMember.IsInherited) {
        // role members supercede base class members. Composition wins over inheritance!
        var method = member as MethodDefinition;
        if (method != null && method.IsVirtual && !method.IsFinal) { 
          // reuses the virtual slot from the base class virtual method
          memberGroup.ReuseSlot = true;
        }

        // if all members in the group are abstract, supercede with the inherited member
        // TODO: what if the inherited member is also abstract?
        var messages = memberGroup.Process().Messages;
        if (messages.Count() == 1 && messages.First().Number == (int)Error.Code.DoesNotImplementAbstractRoleMember) {
          // TODO: issue an info message that the role method is being silently superceded
          memberGroup.MarkAsSuperceded(classMember);
        }

        return;
      }

      // TODO: DECIDE on the spelling: supersede vs supercede!!
      memberGroup.MarkAsSuperceded(classMember);
      if (!member.IsMarkedAsSupersede(TargetType.Module)) {
        // TODO: add a warning?
      }
    }

    // TODO: shouldn't the indexer return from the _conflictGroups list?
    public RoleCompositionMember this[IMemberDefinition memberDefinition] {
      get {
        return _members.Single(roleMember => roleMember.Definition == memberDefinition);
      }
    }

    public ContributedConflictGroup ResolveGroup(IMemberDefinition memberDefinition) {
      return _conflictGroups.SingleOrDefault(group => group.Matches(new RoleMember(memberDefinition.DeclaringType, memberDefinition)));
    }

    public IList<ContributedConflictGroup> RetrieveMemberGroups() {
      return _conflictGroups;
    }

    public void Clear() {
      _members.Clear();
      _conflictGroups.Clear();
    }

    List<Message> _messages = new List<Message>();
    public IEnumerable<Message> Messages {
      get {
        var messageContainers = _members.Cast<IMessageContainer>();
        return messageContainers.SelectMany(mc => mc.Messages).Concat(_messages);
      }
    }
    public void AddMessage(Message message) {
      _messages.Add(message);
    }

    internal void TraceGroups(IConflictClassifier classifier) {
      Tracer.TraceVerbose("[Classifier Groups] : {0}", classifier);
      classifier.Groups.ForEach(group => {
        Tracer.TraceVerbose(group.ToString());
      });
      Tracer.TraceVerbose("[/Classifier Groups]");
    }
    internal void TraceGroups() {
      Tracer.TraceVerbose("[Member Groups]");
      _conflictGroups.ForEach(group => {
        Tracer.TraceVerbose(group.ToString());
      });
      Tracer.TraceVerbose("[/Member Groups]");
    }

  }

}
