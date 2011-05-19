using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace NRoles.Engine {

  /// <summary>
  /// Extracts the code class from a role class.
  /// </summary>
  public class ExtractCodeClassMutator : ExtractTypeMutator {

    /// <summary>
    /// Creates the visitor necessary to extract the code class from a role class.
    /// </summary>
    /// <param name="parameters">Parameters for the extraction.</param>
    /// <param name="result">Result of the extraction.</param>
    /// <returns>The visitor for the extraction.</returns>
    protected override ITypeVisitor CreateVisitor(ExtractTypeParameters parameters, ExtractTypeResult result) {
      return new ExtractCodeClassVisitor(parameters, result);
    }

    /// <summary>
    /// Visitor that extracts the code class from a role class.
    /// </summary>
    class ExtractCodeClassVisitor : ExtractTypeMutatorVisitor {

      /// <summary>
      /// Creates a new instance of this class.
      /// </summary>
      /// <param name="parameters">Parameters to the extraction.</param>
      /// <param name="result">Result of the extraction.</param>
      public ExtractCodeClassVisitor(ExtractTypeParameters parameters, ExtractTypeResult result) :
       base(parameters, result) { }

      #region Type

      /// <summary>
      /// Starts the code class extraction from the given role class.
      /// </summary>
      /// <param name="sourceType">The role class to extract the code class from.</param>
      public override void Visit(TypeDefinition sourceType) {
        Tracer.TraceVerbose("Extract code class: {0} => {1}", sourceType.ToString(), TargetTypeName);

        TargetType = new TypeDefinition(
          string.Empty,
          TargetTypeName,
          TypeAttributes.NestedPublic |
          TypeAttributes.Abstract | TypeAttributes.Sealed |
          TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
          sourceType.Module.Import(typeof(object))
        );
        //CreateConstructor(); // TODO: create a constructor for the static class? make it private?

        if (!SourceType.Methods.Any(method => method.IsConstructor && !method.IsStatic)) {
          // there are no constructors in the source type, but to be consistent, and to enable compatibility on new versions, 
          // we still need to have an Init method in the static class
          CreateDefaultInitMethod();
        }

      }

      public override void Visit(Collection<GenericParameter> genericParameterCollection) {
        TargetType.CopyGenericParametersFrom(genericParameterCollection);
      }

      #endregion

      #region Events

      public override void Visit(Collection<EventDefinition> eventDefinitionCollection) {
        foreach (var eventDefinition in eventDefinitionCollection) {
          // the event is not extracted, only its accessor methods
          if (eventDefinition.AddMethod != null) {
            ExtractAndAddMethod(eventDefinition.AddMethod);
          }
          if (eventDefinition.RemoveMethod != null) {
            ExtractAndAddMethod(eventDefinition.RemoveMethod);
          }
          if (eventDefinition.InvokeMethod != null) { // TODO: need VB.NET to test this?
            ExtractAndAddMethod(eventDefinition.InvokeMethod);
          }
        }
      }

      #endregion

      #region Properties

      public override void Visit(Collection<PropertyDefinition> propertyDefinitionCollection) {
        foreach (PropertyDefinition propertyDefinition in propertyDefinitionCollection) {
          // the property is not extracted, only its accessor methods
          if (propertyDefinition.GetMethod != null) {
            ExtractAndAddMethod(propertyDefinition.GetMethod);
          }
          if (propertyDefinition.SetMethod != null) {
            ExtractAndAddMethod(propertyDefinition.SetMethod);
          }
        }
      }

      #endregion

      #region Methods

      private void CreateDefaultInitMethod() {
        Tracer.TraceVerbose("Create default initialization method");

        var initMethod = new MethodDefinition(
          NameProvider.GetCodeClassInitMethodName(SourceType.Name),
          MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
          SourceType.Module.Import(typeof(void)));

        CreateFirstParameter(initMethod);
        var IL = initMethod.Body.GetILProcessor();
        IL.Emit(OpCodes.Ret);

        TargetType.Methods.Add(initMethod);
      }

      public override void Visit(Collection<MethodDefinition> methodDefinitionCollection) {
        foreach (var sourceMethod in methodDefinitionCollection) {
          if (sourceMethod.IsPropertyAccessor()) {
            continue;
          }

          if (sourceMethod.IsEventAccessor()) {
            continue;
          }
          
          ExtractAndAddMethod(sourceMethod);
        }
      }

      private void ExtractAndAddMethod(MethodDefinition methodDefinition) {
        var staticMethod = ExtractMethod(methodDefinition);
        if (staticMethod != null) {
          TargetType.Methods.Add(staticMethod);
        }
      }

      private MethodDefinition ExtractMethod(MethodDefinition sourceMethod) {
        if (sourceMethod.IsAbstract) { return null; }

        Tracer.TraceVerbose("Extract method: {0}", sourceMethod.ToString());

        string methodName = sourceMethod.Name;
        if (sourceMethod.IsConstructor && !sourceMethod.IsStatic) {
          methodName = NameProvider.GetCodeClassInitMethodName(SourceType.Name); // TODO: look for clashes (signature clashes - also check the parameters -- there might be overloads)!
        }

        if (sourceMethod.IsStatic && sourceMethod.IsPublic) {
          Result.AddMessage(Warning.PublicStaticMethodRelocation(sourceMethod));
        }

        var accessibility =
          sourceMethod.IsConstructor ||
          sourceMethod.IsPublic ||
          sourceMethod.IsFamily || // protected becomes public
          sourceMethod.IsFamilyOrAssembly // protected internal becomes public
          ? MethodAttributes.Public
          : sourceMethod.IsAssembly || sourceMethod.IsFamilyAndAssembly
            ? MethodAttributes.Family
            : MethodAttributes.Private;

        var staticMethod = new MethodDefinition(
          methodName,
          accessibility |
            MethodAttributes.HideBySig | MethodAttributes.Static,
          sourceMethod.ReturnType);

        if (sourceMethod.IsConstructor && sourceMethod.IsStatic) {
          staticMethod.Attributes |= 
            MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        }

        staticMethod.CopyGenericParametersFrom(sourceMethod);
        ExtractMethodParameters(sourceMethod, staticMethod);
        ExtractMethodBody(sourceMethod, staticMethod);

        return staticMethod;
      }

      private void ExtractMethodParameters(MethodDefinition sourceMethod, MethodDefinition staticMethod) {
        if (!sourceMethod.IsStatic) {
          CreateFirstParameter(staticMethod);
        }
        else { 
          // TODO: detect clashes between the existing static method and the new static methods in the class! What to do in the case of clashes?
        }

        foreach (ParameterDefinition parameter in sourceMethod.Parameters) {
          // TODO: parameter custom attributes!
          staticMethod.Parameters.Add(new ParameterDefinition(
            parameter.Name,
            parameter.Attributes,
            parameter.ParameterType));
        }
        // TODO: what about the method's custom attributes???
      }

      private void CreateFirstParameter(MethodDefinition staticMethod) {
        // the first parameter substitutes the instance "this", and refers to the originating class
        var thisType = (TypeReference)SourceType;
        if (TargetType.HasGenericParameters) {
          thisType = new GenericInstanceType(thisType).CopyGenericParametersAsArgumentsFrom(TargetType);
        }
        var thisParameter = new ParameterDefinition(
          NameProvider.GetInstanceParameterName(), // TODO: name clashes with the other parameters? Debugger support?
          ParameterAttributes.None,
          thisType);
        staticMethod.Parameters.Add(thisParameter);
      }

      private void ExtractMethodBody(MethodDefinition sourceMethod, MethodDefinition staticMethod) {
        // copy the method's body
        staticMethod.Body = sourceMethod.Body; // TODO: clone?

        if (sourceMethod.IsStatic || sourceMethod.IsPrivate) {
          // TODO: adjust internal method calls? that depends if we want internal methods on the interface or not. Not in the interface is better!
          AdjustMethodCalls(sourceMethod, staticMethod);
        }
        AdjustCallsToSourceClass(staticMethod.Body);
        if (sourceMethod.IsConstructor) {
          // remove base class constructor call ( : base(...) )
          RemoveBaseClassConstructorCall(staticMethod);
          // TODO: remove other constructor calls ( : this(...) ) ?
        }
      }

      private void RemoveBaseClassConstructorCall(MethodDefinition constructor) {
        var iw = constructor.FindBaseCtorCallInstruction();
        if (iw == null) return;

        var baseCtorInstruction = iw.First;
        var IL = iw.Second;

        if (((MethodReference)baseCtorInstruction.Operand).Resolve().HasParameters) {
          throw new NotSupportedException();
        }

        IL.Remove(baseCtorInstruction.Previous);
        IL.Remove(baseCtorInstruction);
      }

      private void AdjustMethodCalls(MethodDefinition sourceMethod, MethodDefinition staticMethod) {
        // change private method calls to call their static counterpart
        // TODO: what about protected and internal (scan the whole assembly to change then)? they can be virtual, and could be overwritten...
        var codeMutator = new ChangeMethodReferenceVisitor(sourceMethod, staticMethod);
        if (sourceMethod.IsPrivate) {
          Parameters.Context.CodeVisitorsRegistry.Register(codeMutator, TargetType);
        }
        else {
          Parameters.Context.CodeVisitorsRegistry.Register(codeMutator);
        }
      }

      private void AdjustCallsToSourceClass(MethodBody body) {
        // change calls (call) to @this to virtual calls (callvirt), since @this will become an interface type
        // NOTE: this code has special knowledge about the fact that @this will become an interface!
        Parameters.Context.CodeVisitorsRegistry.Register(
          new ChangeCallToCallVirtVisitor(SourceType)); // visit the whole assembly
      }

      #endregion

    }

  }

}
