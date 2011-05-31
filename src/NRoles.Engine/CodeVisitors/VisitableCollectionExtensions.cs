using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace NRoles.Engine {

  static class VisitableCollectionExtensions {
    public static void Accept(this Collection<Instruction> col, ICodeVisitor visitor) {
      col.ToList().ForEach(e => visitor.VisitInstruction(e));
    }
    public static void Accept(this Collection<VariableDefinition> col, ICodeVisitor visitor) {
      col.ToList().ForEach(e => visitor.VisitVariableDefinition(e));
    }
    public static void Accept(this Collection<ExceptionHandler> col, ICodeVisitor visitor) {
      col.ToList().ForEach(e => visitor.VisitExceptionHandler(e));
    }
  }

}
