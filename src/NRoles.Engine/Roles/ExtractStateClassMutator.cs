using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace NRoles.Engine {

  /// <summary>
  /// Extracts the state class from a role class.
  /// </summary>
  public class ExtractStateClassMutator : ExtractTypeMutator {
    
    /// <summary>
    /// Creates the visitor necessary to extract the state class from a role class.
    /// </summary>
    /// <param name="parameters">Parameters for the extraction.</param>
    /// <param name="result">Result of the extraction.</param>
    /// <returns>The visitor for the extraction.</returns>
    protected override ITypeVisitor CreateVisitor(ExtractTypeParameters parameters, ExtractTypeResult result) {
      return new ExtractStateClassVisitor(parameters, result);
    }

    /// <summary>
    /// Visitor that extracts the state class from a role class.
    /// </summary>
    class ExtractStateClassVisitor : ExtractTypeMutatorVisitor {

      /// <summary>
      /// Creates a new instance of this class.
      /// </summary>
      /// <param name="parameters">Parameters to the extraction.</param>
      /// <param name="result">Result of the extraction.</param>
      public ExtractStateClassVisitor(ExtractTypeParameters parameters, ExtractTypeResult result) :
        base(parameters, result) { }

      private PropertyDefinition _stateProperty;

      /// <summary>
      /// Starts the state class extraction from the given role class.
      /// </summary>
      /// <param name="sourceType">The role class to extract the state class from.</param>
      public override void Visit(TypeDefinition sourceType) {
        Tracer.TraceVerbose("Extract state class: {0} => {1}", sourceType.ToString(), TargetTypeName);

        TargetType = new TypeDefinition(
          string.Empty,
          TargetTypeName,
          TypeAttributes.NestedPublic |
          TypeAttributes.Sealed |
          TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
          sourceType.Module.Import(typeof(object))
        );
        TargetType.CopyGenericParametersFrom(sourceType);
        AddStateProperty();
        CreateDefaultConstructor();
      }

      private string DetermineStatePropertyName() {
        return NameProvider.GetStateClassPropertyName(SourceType.Name); // TODO: look for name clashes?
      }

      /// <summary>
      /// Creates and adds the state property, that will be of the extracted state class type,
      /// to the source role class.
      /// </summary>
      private void AddStateProperty() {
        TypeReference targetType = TargetType;
        if (TargetType.HasGenericParameters) {
          targetType = new GenericInstanceType(TargetType).CopyGenericParametersAsArgumentsFrom(TargetType);
        }
        _stateProperty = new PropertyDefinition(
          DetermineStatePropertyName(),
          PropertyAttributes.None,
          targetType);

        _stateProperty.GetMethod = new MethodDefinition(
          "get_" + _stateProperty.Name,
          MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
          MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.SpecialName,
          targetType) {
            SemanticsAttributes = MethodSemanticsAttributes.Getter,
          };
        
        // this property should not conflict with any other member from other types, so mark it as hidden
        _stateProperty.MarkAsHidden(SourceType.Module); // TODO: only the getter should be marked?
        _stateProperty.GetMethod.MarkAsHidden(SourceType.Module);

        SourceType.Properties.Add(_stateProperty);
        SourceType.Methods.Add(_stateProperty.GetMethod);
      }

      public override void Visit(Collection<FieldDefinition> fieldDefinitionCollection) {
        foreach (var fieldDefinition in fieldDefinitionCollection) {
          ExtractField(fieldDefinition);

          var fieldAdjuster = new ChangeFieldReferencesVisitor(fieldDefinition, TargetType, _stateProperty.GetMethod);
          if (fieldDefinition.IsPrivate) {
            Parameters.Context.CodeVisitorsRegistry.Register(fieldAdjuster, scopedAtType: SourceType);
          }
          else {
            Parameters.Context.CodeVisitorsRegistry.Register(fieldAdjuster);
          }
        }
      }

      /// <summary>
      /// Extracts a field from the source role class to the extracted state class.
      /// </summary>
      /// <param name="oldField">Field to extract.</param>
      private void ExtractField(FieldDefinition oldField) {
        // TODO: readonly fields??
        Tracer.TraceVerbose("Extract field: {0}", oldField.Name);
        var newField = new FieldDefinition(
          oldField.Name,
          oldField.Attributes,
          oldField.FieldType);
        if (oldField.HasConstant) {
          newField.Constant = oldField.Constant;
        }
        // TODO: custom attributes?

        // ajust accessibility: all non-public fields become internal (internal/private to the Role interface would be better, but it's not an option in the CLR)
        /*
         NOTE: to use private fields, the classes could be arranged like this:
            interface MyRole {
              public class Data {
                private fields...
                public static class Code { }
              }
            }          
        */
        newField.Attributes &= ~(FieldAttributes.Assembly | FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.FamORAssem | FieldAttributes.Private | FieldAttributes.Public);
        if (oldField.IsPublic) {
          newField.Attributes |= FieldAttributes.Public;
        }
        else {
          newField.Attributes |= FieldAttributes.Assembly;
        }

        if (oldField.IsPublic) { 
          // TODO: assert that the field is static
          // TODO: this is also applicable if the old field is internal and the assembly is marked with InternalsVisibleTo
          Result.AddMessage(Warning.PublicStaticFieldRelocation(oldField));
        }

        TargetType.Fields.Add(newField);
      }

    }

  }

}
