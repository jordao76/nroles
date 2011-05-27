using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  class FindRoleInheritance : IAssemblyTypesVisitor {

    private readonly TypeDefinition _roleType;
    private readonly MutationContext _context;

    public FindRoleInheritance(TypeDefinition roleType, MutationContext context) {
      _roleType = roleType;
      _context = context;
    }

    public void Visit(TypeDefinition type) {
      if (type.BaseType != null && type.BaseType.Resolve() == _roleType) {
        _context.AddMessage(Error.TypeCantInheritFromRole(type, _roleType));
      }
    }

    public void Visit(AssemblyDefinition assembly) { }
  }

}
