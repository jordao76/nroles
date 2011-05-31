using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  class ChangeFieldReferencesToPropertyVisitor : CodeVisitorBase {

    FieldDefinition _field;
    PropertyDefinition _property;

    public ChangeFieldReferencesToPropertyVisitor(FieldDefinition field, PropertyDefinition property) {
      if (field == null) throw new ArgumentNullException("field");
      if (property == null) throw new ArgumentNullException("property");
      _field = field;
      _property = property;
    }

    public override void VisitInstruction(Instruction instruction) {
      var field = instruction.Operand as FieldReference;
      if (field != null && field.Resolve() == _field) {

        if (instruction.IsFieldLoad()) {
          instruction.Operand = ResolveGetter(field);
        }
        else {
          instruction.Operand = ResolveSetter(field);
        }

        instruction.OpCode = OpCodes.Callvirt;

      }
    }

    private MethodReference ResolveSetter(FieldReference field) {
      var setter = new MethodReference(
        _property.SetMethod.Name,
        _field.DeclaringType.Module.Import(typeof(void))) {
          DeclaringType = field.DeclaringType,
          HasThis = _property.SetMethod.HasThis,
          ExplicitThis = _property.SetMethod.ExplicitThis,
          CallingConvention = _property.SetMethod.CallingConvention
        };
      setter.Parameters.Add(new ParameterDefinition(field.FieldType));
      return setter;
    }

    private MethodReference ResolveGetter(FieldReference field) {
      return new MethodReference(
        _property.GetMethod.Name,
        field.FieldType) {
          DeclaringType = field.DeclaringType,
          HasThis = _property.GetMethod.HasThis,
          ExplicitThis = _property.GetMethod.ExplicitThis,
          CallingConvention = _property.GetMethod.CallingConvention
        };
    }

  }

}
