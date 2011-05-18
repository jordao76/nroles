using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace NRoles.Engine {

  // Migration: copied from Mono.Cecil 0.6 
  public interface ICodeVisitor {
    void TerminateMethodBody(MethodBody body);
    void VisitExceptionHandler(ExceptionHandler eh);
    void VisitExceptionHandlerCollection(Collection<ExceptionHandler> seh);
    void VisitInstruction(Instruction instr);
    void VisitInstructionCollection(Collection<Instruction> instructions);
    void VisitMethodBody(MethodBody body);
    void VisitScope(Scope scope);
    void VisitVariableDefinition(VariableDefinition var);
    void VisitVariableDefinitionCollection(Collection<VariableDefinition> variables);
  }

  public abstract class CodeVisitorBase : ICodeVisitor {
    
    public virtual void VisitMethodBody(MethodBody body) { }
    public virtual void TerminateMethodBody(MethodBody body) { }

    public void VisitInstructionCollection(Collection<Instruction> instructions) {
      instructions.Accept(this);
    }
    public virtual void VisitInstruction(Instruction instruction) { }

    public void VisitVariableDefinitionCollection(Collection<VariableDefinition> variables) {
      variables.Accept(this);
    }
    public virtual void VisitVariableDefinition(VariableDefinition variable) { }

    public virtual void VisitScope(Scope scope) { }

    public void VisitExceptionHandlerCollection(Collection<ExceptionHandler> seh) {
      seh.Accept(this);
    }
    public virtual void VisitExceptionHandler(ExceptionHandler eh) { }

  }

}
