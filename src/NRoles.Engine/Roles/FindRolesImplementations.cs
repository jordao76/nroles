using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  class FindRolesImplementations : IAssemblyTypesVisitor {
    private readonly MutationContext _context;

    public FindRolesImplementations(MutationContext context) {
      _context = context;
    }

    public void Visit(TypeDefinition type) {
      if (type.IsRoleView()) return;
      type.Interfaces.
        Select(interfaceReference => interfaceReference.Resolve()).
        Where(interfaceType => interfaceType.IsRole()).
        ForEach(roleType =>
          _context.AddMessage(Error.TypeCantInheritFromRole(type, roleType)));
    }

    public void Visit(AssemblyDefinition assembly) { }
  }

}
