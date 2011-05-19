using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  public abstract class CompositeCodeVisitorBase : CodeVisitorBase {
    public abstract void AddCodeVisitor(CodeVisitorBase visitor);
    internal abstract void Apply(Action<CodeVisitorBase> action);

    public override void VisitMethodBody(MethodBody body) {
      Apply(visitor => visitor.VisitMethodBody(body));
    }
    public override void TerminateMethodBody(MethodBody body) {
      Apply(visitor => visitor.TerminateMethodBody(body));
    }
    public override void VisitInstruction(Instruction instruction) {
      Apply(visitor => visitor.VisitInstruction(instruction));
    }
    public override void VisitVariableDefinition(VariableDefinition variable) {
      Apply(visitor => visitor.VisitVariableDefinition(variable));
    }
    public override void VisitScope(Scope scope) {
      Apply(visitor => visitor.VisitScope(scope));
    }
    public override void VisitExceptionHandler(ExceptionHandler eh) {
      Apply(visitor => visitor.VisitExceptionHandler(eh));
    }
  }

}
