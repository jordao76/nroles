using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  partial class RoleComposer {

    sealed partial class MemberComposer {

      private MethodDefinition RoleMethod { get { return (MethodDefinition)_roleMember.Definition; } }

      private MethodDefinition ImplementPropertyAccessor() {
        if (_backingField != null) {
          return ImplementAutoPropertyAccessor();
        }
        else {
          return ImplementMethodCallingCodeClass();
        }
      }

      private MethodDefinition ImplementMethodCallingCodeClass() {
        return ImplementMethod(CreateCodeToCallCodeClass);
      }

      private MethodDefinition ImplementAutoPropertyAccessor() {
        return ImplementMethod(CreateCodeForAutoPropertyAccessor);
      }

      private MethodDefinition ImplementMethod(Action<MethodDefinition> createMethodCode) {
        var implementedMethod = ComposeMethod(createMethodCode);
        AddOverrides(implementedMethod, _overrides);
        return implementedMethod;
      }

      private void AddOverrides(MethodDefinition method, IEnumerable<RoleCompositionMember> overrides) {
        overrides.ForEach(overriddenMember => {
          var overrideReference = ResolveMethodReference(
            overriddenMember.Role,
            (MethodDefinition)overriddenMember.Definition, @override: true);
          Tracer.TraceVerbose("Overrides: {0}", overrideReference);
          method.Overrides.Add(overrideReference);
        });
      }

      private MethodReference ResolveMethodReference(TypeReference role, MethodDefinition methodDefinition, bool @override = false) {
        // TODO: create class field for "new MemberResolver(Role, Module)"
        return new MemberResolver(role, Module).ResolveMethodReference(methodDefinition, @override);
      }

      private MethodDefinition ComposeMethod(Action<MethodDefinition> createMethodCode) {
        Tracer.TraceVerbose("Compose method: {0}", _name);

        var placeholder = ((MethodDefinition)Group.Placeholder);

        // TODO: create class field for "new MemberResolver(Role)"
        var implementedMethod = placeholder ?? 
          new MemberResolver(Role, Module).ResolveMethodDefinition(
            RoleMethod, 
            _name,
            _accessSpecifier);

        implementedMethod.Attributes |=
            MethodAttributes.HideBySig | // TODO: what about HideByName?
            //MethodAttributes.Final |  // TODO?
            MethodAttributes.NewSlot |
            MethodAttributes.Virtual;

        implementedMethod.SemanticsAttributes = RoleMethod.SemanticsAttributes;

        createMethodCode(implementedMethod);

        if (placeholder == null) {
          TargetType.Methods.Add(implementedMethod);
        }

        return implementedMethod;
      }

      private void PushParameters(MethodDefinition implementedMethod) {
        // push all parameters on the stack
        var worker = implementedMethod.Body.GetILProcessor();
        worker.Emit(OpCodes.Ldarg_0); // push "this"
        for (int paramIndex = 1; paramIndex <= implementedMethod.Parameters.Count; ++paramIndex) {
          var paramReference = implementedMethod.Parameters[paramIndex - 1];
          Instruction paramPushInstruction;
          switch (paramIndex) {
            case 1: paramPushInstruction = worker.Create(OpCodes.Ldarg_1); break;
            case 2: paramPushInstruction = worker.Create(OpCodes.Ldarg_2); break;
            case 3: paramPushInstruction = worker.Create(OpCodes.Ldarg_3); break;
            default: paramPushInstruction = worker.Create(OpCodes.Ldarg_S, paramReference); break; // TODO: Ldarg_S or Ldarg?
          }
          worker.Append(paramPushInstruction);
        }
      }

      #region Calling code class method

      private void CreateCodeToCallCodeClass(MethodDefinition implementedMethod) {
        var companionClassMethod = ResolveCompanionMethod();
        implementedMethod.Body = new MethodBody(implementedMethod);
        PushParameters(implementedMethod);
        EmitCodeToCallMethod(implementedMethod, companionClassMethod);
        implementedMethod.Body.GetILProcessor().Emit(OpCodes.Ret);
      }

      private void EmitCodeToCallMethod(MethodDefinition caller, MethodReference callee) {
        var worker = caller.Body.GetILProcessor();
        //worker.Emit(OpCodes.Tail); // TODO: cannot pass ByRef to a tail call
        worker.Emit(OpCodes.Call, callee);
      }

      private MethodReference ResolveCompanionMethod() {
        var correspondingMethod = Role.Resolve().ResolveCorrespondingMethod(RoleMethod);
        if (correspondingMethod == null) throw new InvalidOperationException();
        var methodReference = ResolveMethodReference(Role, correspondingMethod);
        return methodReference;
      }

      #endregion

      #region Auto property methods

      private void CreateCodeForAutoPropertyAccessor(MethodDefinition implementedMethod) {
        if (implementedMethod.IsAbstract) return;

        if (implementedMethod.IsPropertyGetter()) {
          EmitGetterCode(implementedMethod);
        }
        else if (implementedMethod.IsPropertySetter()) {
          EmitSetterCode(implementedMethod);
        }
      }

      private void EmitGetterCode(MethodDefinition implementedMethod) {
        if (_backingField == null) throw new InvalidOperationException();
        var IL = implementedMethod.Body.GetILProcessor();
        IL.Emit(OpCodes.Ldarg_0);
        IL.Emit(OpCodes.Ldfld, ResolveFieldReference(_backingField));
        IL.Emit(OpCodes.Ret);
      }

      private void EmitSetterCode(MethodDefinition implementedMethod) {
        if (_backingField == null) throw new InvalidOperationException();
        var IL = implementedMethod.Body.GetILProcessor();
        IL.Emit(OpCodes.Ldarg_0);
        IL.Emit(OpCodes.Ldarg_1);
        IL.Emit(OpCodes.Stfld, ResolveFieldReference(_backingField));
        IL.Emit(OpCodes.Ret);
      }

      private FieldReference ResolveFieldReference(FieldDefinition fieldDefinition) {
        return new FieldReference(
          fieldDefinition.Name,
          fieldDefinition.FieldType) {
            DeclaringType = fieldDefinition.DeclaringType.ResolveGenericArguments()
        };
      }

      #endregion

    }
  
  }

}
