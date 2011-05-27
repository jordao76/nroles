using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  class ChangeMethodReferenceVisitor : CodeVisitorBase {
    private MethodDefinition _sourceMethod;
    private MethodDefinition _targetMethod;
    private ModuleDefinition Module { get { return _sourceMethod.DeclaringType.Module; } }
    public ChangeMethodReferenceVisitor(MethodDefinition sourceMethod, MethodDefinition targetMethod) {
      _sourceMethod = sourceMethod;
      _targetMethod = targetMethod;
    }
    public override void VisitInstruction(Instruction instruction) {
      if (instruction.Operand is MethodReference) {
        var methodReference = (MethodReference)instruction.Operand;
        if (IsSourceMethod(methodReference)) {
          instruction.Operand =
            // the new method reference must have the same generics context as the old one
            new MemberResolver(methodReference.DeclaringType, Module).ResolveMethodReference(_targetMethod);
        }
      }
    }
    bool IsSourceMethod(MethodReference method) {
      return method.Resolve() == _sourceMethod;
    }
  }

}
