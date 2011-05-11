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
      var method = instruction.Operand as MethodReference;
      if (method != null && IsSourceMethod(method)) {
        if (instruction.OpCode == OpCodes.Callvirt) {
          // a static method is not virtual
          instruction.OpCode = OpCodes.Call;
        }
        // the new method reference must have the same generics context as the old one
        instruction.Operand =
          new MemberResolver(method.DeclaringType, Module).ResolveMethodReference(_targetMethod);
      }
    }

    bool IsSourceMethod(MethodReference method) {
      return method.Resolve() == _sourceMethod;
    }

  }

}
