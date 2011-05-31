using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  partial class RoleComposer {

    private TypeDefinition _targetType;
    private IList<TypeReference> _roles;
    private RoleCompositionMemberContainer _container;
    private ModuleDefinition Module { get { return _targetType.Module; } }

    public RoleComposer(TypeDefinition targetType, IList<TypeReference> roles, RoleCompositionMemberContainer container) {
      _targetType = targetType;
      _roles = roles;
      _container = container;
    }

    public IOperationResult Compose() {
      var result = new CompositeOperationResult();

      AddRoleInterfaces();

      {
        var composeMemberGroupsResult = ComposeMemberGroups();
        result.AddResult(composeMemberGroupsResult);
        if (!result.Success) return result;
      }

      WeaveInitializationCode();

      return result;
    }

    private void AddRoleInterfaces() {
      _roles.ForEach(role => _targetType.Interfaces.Add(role));
    }

    private IOperationResult ComposeMemberGroups() {
      var result = new CompositeOperationResult();
      var roleMemberGroups = _container.RetrieveMemberGroups();
      var memberComposer = new MemberComposer(_targetType, _container);
      roleMemberGroups.ForEach(group => memberComposer.Compose(group));
      result.Slurp(memberComposer);
      return result;
    }

  }

}
