using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  /// <summary>
  /// Changes all field references (field loads and field stores) on a specific field
  /// to reference the corresponding field in another (target) type, which is accessible through a passed in getter method. 
  /// Assumes that the target type contains the same field definition.
  /// </summary>
  class ChangeFieldReferencesVisitor : CodeVisitorBase {

    FieldDefinition _sourceField;
    TypeReference _target;
    MethodReference _stateGetter;

    public ChangeFieldReferencesVisitor(FieldDefinition sourceField, TypeReference target, MethodDefinition stateGetter) {
      if (sourceField == null) throw new ArgumentNullException("sourceField");
      if (target == null) throw new ArgumentNullException("target");
      if (stateGetter == null) throw new ArgumentNullException("stateField");
      if (sourceField.DeclaringType == target.Resolve()) throw new InvalidOperationException();
      _sourceField = sourceField;
      _target = target;
      _stateGetter = ResolveStateGetter(stateGetter);
    }

    MethodBody _body;
    ILProcessor _il;

    public override void VisitMethodBody(MethodBody body) {
      _body = body;
      _il = body.GetILProcessor();
    }

    public override void VisitInstruction(Instruction instruction) {
      var field = instruction.Operand as FieldReference;
      if (field != null && field.Resolve() == _sourceField) {

        if (!_sourceField.IsStatic) {

          Instruction instructionAfterGetter;
          if (instruction.IsFieldLoad()) {
            instructionAfterGetter = instruction;
          }
          else {
            instructionAfterGetter = ProcessFieldStore(instruction, field);
          }
          _il.InsertBefore(instructionAfterGetter,
          _il.Create(OpCodes.Callvirt, _stateGetter));

        }

        instruction.Operand = new FieldReference(
          field.Name,
          field.FieldType) {
            DeclaringType = _target.ResolveGenericArguments()
          };
      }
    }

    private Instruction ProcessFieldStore(Instruction fieldStoreInstruction, FieldReference field) {
      // add a new local variable to hold the top of the stack
      _body.InitLocals = true;
      var variable = new VariableDefinition(field.FieldType);
      _body.Variables.Add(variable);

      Instruction storeLocalInstruction = CreateStoreLocalInstruction(variable);
      Instruction loadLocalInstruction = CreateLoadLocalInstruction(variable);

      _il.InsertBefore(fieldStoreInstruction, loadLocalInstruction);
      _il.InsertBefore(loadLocalInstruction, storeLocalInstruction);

      Instruction instructionAfterGetter = loadLocalInstruction;
      return instructionAfterGetter;
    }

    private Instruction CreateLoadLocalInstruction(VariableDefinition variable) {
      Instruction instruction;
      switch (variable.Index) {
        case 0: instruction = _il.Create(OpCodes.Ldloc_0); break;
        case 1: instruction = _il.Create(OpCodes.Ldloc_1); break;
        case 2: instruction = _il.Create(OpCodes.Ldloc_2); break;
        case 3: instruction = _il.Create(OpCodes.Ldloc_3); break;
        default: instruction = _il.Create(OpCodes.Ldloc_S, variable.Index); break; // TODO: Ldloc or Ldloc_S ?
      }
      return instruction;
    }

    private Instruction CreateStoreLocalInstruction(VariableDefinition variable) {
      Instruction instruction;
      switch (variable.Index) {
        case 0: instruction = _il.Create(OpCodes.Stloc_0); break;
        case 1: instruction = _il.Create(OpCodes.Stloc_1); break;
        case 2: instruction = _il.Create(OpCodes.Stloc_2); break;
        case 3: instruction = _il.Create(OpCodes.Stloc_3); break;
        default: instruction = _il.Create(OpCodes.Stloc_S, variable.Index); break; // TODO: Stloc or Stloc_S ?
      }
      return instruction;
    }

    private MethodReference ResolveStateGetter(MethodDefinition stateGetter) {
      return new MethodReference(
        stateGetter.Name,
        _target.ResolveGenericArguments()) {
          DeclaringType = _sourceField.DeclaringType.ResolveGenericArguments(),
          HasThis = stateGetter.HasThis,
          ExplicitThis = stateGetter.ExplicitThis,
          CallingConvention = stateGetter.CallingConvention
        };
    }

  }

}
