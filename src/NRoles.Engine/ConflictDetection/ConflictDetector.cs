using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public class ConflictDetector {

    TypeDefinition _targetType;
    
    public RoleCompositionMemberContainer Container { get; private set; }

    public ConflictDetector(TypeDefinition targetType) {
      if (targetType == null) throw new ArgumentNullException("targetType");
      _targetType = targetType;
      Container = new RoleCompositionMemberContainer(targetType);
    }

    public ConflictDetectionResult Process(params TypeReference[] roles) {
      return Process(roles.ToList());
    }

    public ConflictDetectionResult Process(List<TypeReference> roles) {
      if (roles == null) throw new ArgumentNullException("roles");
      Container.Clear(); // this method (Process) should be idempotent

      var result = new ConflictDetectionResult();

      roles.ForEach(role => AddRole(role));

      Container.Process();

      Container.TraceGroups();

      result.Slurp(Container);

      return result;
    }

    private void AddRole(TypeReference role) {
      var memberReader = new MemberReaderVisitor();
      role.Resolve().Accept(memberReader);
      var roleMembers = memberReader.Members.Select(member => MakeRoleMember(role, member));
      AddRoleMembers(role, roleMembers);
    }

    private void AddRoleMembers(TypeReference role, IEnumerable<RoleCompositionMember> members) {
      members.ForEach(member => AddRoleMember(role, member));
    }

    private void AddRoleMember(TypeReference role, RoleCompositionMember member) {
      Container.AddMember(member);
    }

    private RoleCompositionMember MakeRoleMember(TypeReference role, IMemberDefinition member) {
      if (member.IsInRoleView()) {
        return new RoleViewMember(role, member);
      }
      else {
        return new RoleMember(role, member);
      }
    }

    public IList<ContributedConflictGroup> RetrieveMemberGroups() {
      return Container.RetrieveMemberGroups();
    }

  }

  public class ConflictDetectionResult : CompositeOperationResult {
    public bool HasConflict { get { return !Success; } }
  }

}
