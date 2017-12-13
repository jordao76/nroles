using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  class FindRoleInstantiation : CodeVisitorBase {
    private TypeDefinition _roleType;
    private MutationContext _context;

    public FindRoleInstantiation(TypeDefinition roleType, MutationContext context) {
      _roleType = roleType;
      _context = context;
    }

    MethodDefinition _currentMethod;

    public override void VisitMethodBody(MethodBody body) {
      _currentMethod = body.Method;
    }

    public override void VisitInstruction(Instruction instruction) {
      if (instruction.Operand is MethodReference method) {
        var methodDefinition = method.Resolve();
        if (methodDefinition.Name == ".ctor" && methodDefinition.DeclaringType == _roleType) {
          _context.AddMessage(Error.RoleInstantiated(_roleType, _currentMethod, instruction.SequencePoint));
        }
      }
    }
  }

}
