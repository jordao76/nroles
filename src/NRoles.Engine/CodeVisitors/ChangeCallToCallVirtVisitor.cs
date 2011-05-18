using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  class ChangeCallToCallVirtVisitor : CodeVisitorBase {

    private TypeDefinition _sourceType;
    
    public ChangeCallToCallVirtVisitor(TypeDefinition sourceType) {
      _sourceType = sourceType;
    }

    public override void VisitInstruction(Instruction instruction) {
      var method = instruction.Operand as MethodReference;
      if (method != null && ShouldChangeToCallVirt(method)) {
        if (instruction.OpCode == OpCodes.Call) {
          instruction.OpCode = OpCodes.Callvirt;
        }
      }
    }

    protected bool ShouldChangeToCallVirt(MethodReference method) {
      return
        method.DeclaringType.Resolve() == _sourceType &&
        method.Resolve().RemainsInRoleInterface();
    }

  }

}
