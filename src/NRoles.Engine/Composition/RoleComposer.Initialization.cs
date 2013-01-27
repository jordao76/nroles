using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  partial class RoleComposer {

    private void WeaveInitializationCode() {

      if (_targetType.IsInterface) {
        // interfaces cannot compose roles
        // TODO: check for this before this code!!
        throw new InvalidOperationException();
      }

      // Strategy for constructors: find a base class constructor call and add the necessary code after the call
      var baseCtorCallInstructions = _targetType.Methods.Where(m => m.IsConstructor && !m.IsStatic). // TODO: get constructors extension method!
        Assert(col => col.Count() >= 1). // there shall be at least ONE constructor in the class // TODO: if no constructor exists in the class, create one... raise an error or add the constructor? what about structs?
        Select(ctor => ctor.FindBaseCtorCallInstruction()).
        Where(ip => ip != null);

      // The necessary code is:
      //   1. create each role State Class
      //   2. call Init on the Code Class for each role

      baseCtorCallInstructions.ForEach(ip => {
        var stateClassInstruction = ip.First;
        Instruction initInstruction = null;
        var il = ip.Second;
        _roles.ForEach(role => {
          if (!role.IsRoleView()) {
            stateClassInstruction = EmitStateClassCreation(role, stateClassInstruction, il);
            if (initInstruction == null) initInstruction = stateClassInstruction;
            initInstruction = EmitCodeInit(role, initInstruction, il);
          }
        });
      });

    }

    private Instruction EmitStateClassCreation(TypeReference role, Instruction instructionBeforeCreation, ILProcessor il) {
      var current = instructionBeforeCreation;
      var stateClassFieldReference = ResolveStateClassField(role);
      // don't emit this code if the state field and property was not implemented
      // TODO: this needs to be checked before, possibly at the conflict resolution stage
      if (stateClassFieldReference != null) {
        current = InsertAfter(il, current, il.Create(OpCodes.Ldarg_0));
        current = InsertAfter(il, current, il.Create(OpCodes.Newobj, role.ResolveStateClassCtor()));
        current = InsertAfter(il, current, il.Create(OpCodes.Stfld, stateClassFieldReference));
      }
      return current;
    }

    private FieldReference ResolveStateClassField(TypeReference role) {
      var stateTypeDefinition = role.ResolveStateClass();
      var stateTypeReference = new MemberResolver(role).ResolveMatchingType(stateTypeDefinition);
      var fieldDefinition = _targetType.Fields.
        SingleOrDefault(fd => TypeMatcher.IsMatch(fd.FieldType, stateTypeReference));
      if (fieldDefinition == null) return null; // TODO: this is because the role was already implemented in a base class; it should be checked before, possibly in the conflict resolution stage
      return new FieldReference(
        fieldDefinition.Name,
        fieldDefinition.FieldType) {
          DeclaringType = fieldDefinition.DeclaringType.ResolveGenericArguments()
      };
    }

    private Instruction EmitCodeInit(TypeReference role, Instruction instructionBeforeInit, ILProcessor il) {
      var current = instructionBeforeInit;
      current = InsertAfter(il, current, il.Create(OpCodes.Ldarg_0));
      current = InsertAfter(il, current, il.Create(OpCodes.Call, ResolveInitReference(role)));
      return current;
    }

    private MethodReference ResolveInitReference(TypeReference role) {
      var codeClassDefinition = role.ResolveCodeClass();
      var initDefinition = codeClassDefinition.Methods.Cast<MethodDefinition>().Single(md => md.Name == NameProvider.GetCodeClassInitMethodName(role.Name));
      return new MemberResolver(role, Module).ResolveMethodReference(initDefinition);
    }

    private Instruction InsertAfter(ILProcessor il, Instruction target, Instruction instruction) {
      il.InsertAfter(target, instruction);
      return instruction;
    }

  }

}
