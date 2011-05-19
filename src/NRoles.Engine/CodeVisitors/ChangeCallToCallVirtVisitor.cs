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
      if (instruction.OpCode == OpCodes.Call && (instruction.Operand is MethodReference) && ShouldChangeToCallVirt((MethodReference)instruction.Operand)) {
        instruction.OpCode = OpCodes.Callvirt;
      }
    }
    protected bool ShouldChangeToCallVirt(MethodReference method) {
      var methodDefinition = method.Resolve();
      return
        method.DeclaringType == _sourceType &&
        // private and static methods should not change to callvirt
        !methodDefinition.IsPrivate && !methodDefinition.IsStatic;
    }
  }

}
