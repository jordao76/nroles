using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {
  
  class FieldToPropertyMutator {
    private TypeDefinition _type;

    public FieldToPropertyMutator(TypeDefinition type) {
      if (type == null) throw new ArgumentNullException("type");
      _type = type;
    }

    MutationContext _context;

    public void Process(MutationContext context) {
      if (context == null) throw new ArgumentNullException("context");
      _context = context;

      _type.Fields.Cast<FieldDefinition>().
        Where(fd => fd.IsPublic && !fd.IsStatic). // TODO: what about internal fields?
        ForEach(fd => Process(fd));
    }

    private void Process(FieldDefinition field) {
      Tracer.TraceVerbose("Mutate field into property: {0}", field.Name);

      // make the field private
      field.Attributes &= ~(FieldAttributes.Assembly | FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.FamORAssem | FieldAttributes.Private | FieldAttributes.Public);
      field.Attributes |= FieldAttributes.Private;

      var fieldProperty = CreateProperty(field);

      _context.CodeVisitorsRegistry.Register(
        new ChangeFieldReferencesToPropertyVisitor(field, fieldProperty));
    }

    private PropertyDefinition CreateProperty(FieldDefinition field) {
      // create a property by the same name that accesses the field
      // TODO: if the field is readonly create only the getter?
      var fieldProperty = new PropertyDefinition(
        field.Name,
        PropertyAttributes.None,
        field.FieldType);

      // TODO: annotate with EditorBrowsableState.Never! CompilerGeneratedAttribute? Debugging attributes?

      CreateGetter(fieldProperty);
      CreateSetter(fieldProperty);

      // add the property to the type
      _type.Properties.Add(fieldProperty);
      _type.Methods.Add(fieldProperty.GetMethod);
      _type.Methods.Add(fieldProperty.SetMethod);

      return fieldProperty;
    }

    private void CreateSetter(PropertyDefinition fieldProperty) {
      // create the setter
      fieldProperty.SetMethod = new MethodDefinition(
        "set_" + fieldProperty.Name, // TODO: look for clashes!!
        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
        MethodAttributes.Virtual | MethodAttributes.SpecialName,
        _type.Module.Import(typeof(void)));
      fieldProperty.SetMethod.SemanticsAttributes = MethodSemanticsAttributes.Setter;
      fieldProperty.SetMethod.IsSetter = true;
      fieldProperty.SetMethod.Parameters.Add(new ParameterDefinition(
        "value", ParameterAttributes.None, // use Unused?
        fieldProperty.PropertyType));

      EmitSetterCode(fieldProperty);
    }

    private void EmitSetterCode(PropertyDefinition fieldProperty) {
      var IL = fieldProperty.SetMethod.Body.GetILProcessor();
      IL.Append(IL.Create(OpCodes.Ldarg_0));
      IL.Append(IL.Create(OpCodes.Ldarg_1));
      IL.Append(IL.Create(OpCodes.Stfld, new FieldReference(
        fieldProperty.Name,
        fieldProperty.PropertyType) {
          DeclaringType = _type.ResolveGenericArguments()
      }));
      IL.Append(IL.Create(OpCodes.Ret));
    }

    private void CreateGetter(PropertyDefinition fieldProperty) {
      // create the getter
      fieldProperty.GetMethod = new MethodDefinition(
        "get_" + fieldProperty.Name, // TODO: look for clashes!!
        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
        MethodAttributes.Virtual | MethodAttributes.SpecialName,
        fieldProperty.PropertyType);
      fieldProperty.GetMethod.SemanticsAttributes = MethodSemanticsAttributes.Getter;
      fieldProperty.GetMethod.IsGetter = true;

      EmitGetterCode(fieldProperty);
    }

    private void EmitGetterCode(PropertyDefinition fieldProperty) {
      var IL = fieldProperty.GetMethod.Body.GetILProcessor();
      IL.Append(IL.Create(OpCodes.Ldarg_0));
      IL.Append(IL.Create(OpCodes.Ldfld, new FieldReference(
        fieldProperty.Name,
        fieldProperty.PropertyType) {
          DeclaringType = _type.ResolveGenericArguments()
      }));
      IL.Append(IL.Create(OpCodes.Ret));
    }

  }

}
