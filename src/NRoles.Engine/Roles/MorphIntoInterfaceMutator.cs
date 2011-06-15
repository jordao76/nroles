using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace NRoles.Engine {

  public class MorphIntoInterfaceMutator {

    public OperationResult Morph(MutationParameters parameters) {
      parameters.Validate();
      if (parameters.SourceType == null) throw new ArgumentException("parameters must contain a SourceType", "parameters");
      var targetType = parameters.SourceType; // the source is also the target
      var visitor = new MorphIntoInterfaceVisitor(parameters);
      targetType.Accept(visitor);
      return visitor.Result;
    }

    class MorphIntoInterfaceVisitor : TypeVisitorBase {

      // TODO: it seems that the context WrapUpActions are only being used by this class. Can it be encapsulated here?

      public readonly OperationResult Result = new OperationResult();
      MutationParameters _parameters;

      public MorphIntoInterfaceVisitor(MutationParameters parameters) {
        if (parameters == null) throw new ArgumentNullException("parameters");
        if (parameters.SourceType == null) throw new ArgumentException("parameters must contain a SourceType", "parameters");
        _parameters = parameters;
      }

      private void AddMessage(Message message) {
        Result.AddMessage(message);
      }

      #region Type

      public override void Visit(TypeDefinition targetType) {
        if (targetType == null) throw new ArgumentNullException("targetType");

        Tracer.TraceVerbose("Morph interface: {0}", targetType.FullName);

        if (!targetType.IsInterface && 
            !InheritsDirectlyFromObject(targetType)) {
          AddMessage(Error.RoleInheritsFromClass(targetType.FullName, targetType.BaseType.FullName));
          return;
        }

        var accessibilityModifiers = targetType.RetrieveAccessibilityModifiers();
        targetType.Attributes = TypeAttributes.Interface | TypeAttributes.Abstract | TypeAttributes.AnsiClass | accessibilityModifiers;
        targetType.BaseType = null;
      }

      private bool InheritsDirectlyFromObject(TypeDefinition targetType) {
        var baseType = targetType.BaseType;
        // Note: comparing TypeDefinitions didn't work!
        return baseType.FullName == "System.Object";
      }

      public override void Visit(Collection<CustomAttribute> customAttributeCollection) {
        foreach (var customAttribute in customAttributeCollection) {
          // TODO: some attributes can be applied to Classes, but NOT to Interfaces!!
        }
      }

      #endregion

      private void Defer(Action action) {
        _parameters.Context.RegisterWrapUpAction(mc => action());
      }

      private bool MorphAccessor(MethodDefinition accessor) {
        var remove = false;
        if (accessor != null) {
          remove = RemoveAccessor(accessor);
          MorphMethod(accessor);
        }
        return remove;
      }

      private bool RemoveAccessor(MethodDefinition accessor) {
        return !accessor.RemainsInRoleInterface();
      }

      #region Properties

      public override void Visit(Collection<PropertyDefinition> propertyDefinitionCollection) {
        var snapshot = propertyDefinitionCollection.ToList();
        snapshot.ForEach(property => MorphProperty(property));
      }

      private void MorphProperty(PropertyDefinition property) {
        Tracer.TraceVerbose("Morph property: {0}", property.Name);

        var removeGetter = MorphAccessor(property.GetMethod);
        if (removeGetter) {
          Defer(() => property.GetMethod = null);
        }

        var removeSetter = MorphAccessor(property.SetMethod);
        if (removeSetter) {
          Defer(() => property.SetMethod = null);
        }

        Defer(() => {
          //if (property.GetMethod == null && property.SetMethod == null) // TODO: Cecil BUG: this condition was causing get_Smile to lose its SemanticsAttributes
          if (removeGetter && removeSetter) {
            property.DeclaringType.Properties.Remove(property);
          }
        });
      }

      #endregion

      #region Fields

      public override void Visit(Collection<FieldDefinition> fieldDefinitionCollection) {
        var fieldsToBeRemoved = fieldDefinitionCollection.ToList();
        Defer(() => {
          fieldsToBeRemoved.ForEach(field => fieldDefinitionCollection.Remove(field));
        });
      }

      #endregion

      #region Events

      public override void Visit(Collection<EventDefinition> eventDefinitionCollection) {
        var snapshot = eventDefinitionCollection.Cast<EventDefinition>().ToList();
        snapshot.ForEach(@event => MorphEvent(@event));
      }

      private void MorphEvent(EventDefinition @event) {
        Tracer.TraceVerbose("Morph event: {0}", @event.Name);

        var removeAdder = MorphAccessor(@event.AddMethod);
        if (removeAdder) {
          Defer(() => @event.AddMethod = null);
        }

        var removeRemover = MorphAccessor(@event.RemoveMethod);
        if (removeRemover) {
          Defer(() => @event.RemoveMethod = null);
        }

        var removeInvoker = MorphAccessor(@event.InvokeMethod);
        if (removeInvoker) {
          Defer(() => @event.InvokeMethod = null);
        }

        Defer(() => {
          if (removeAdder && removeRemover) { // Note: the InvokeMethod is optional, and should not occur alone. So it's not checked here.
            @event.DeclaringType.Events.Remove(@event);
          }
        });
      }

      #endregion

      #region Methods

      public override void Visit(Collection<MethodDefinition> methodDefinitionCollection) {
        var snapshot = methodDefinitionCollection.ToList();
        foreach (var method in snapshot) {

          if (method.IsPropertyAccessor()) {
            continue;
          }

          if (method.IsEventAccessor()) {
            continue;
          }

          MorphMethod(method);
        }
      }

      private void MorphMethod(MethodDefinition method) {
        Tracer.TraceVerbose("Morph method: {0}", method.ToString());

        if (method.IsConstructor && method.HasParameters) {
          AddMessage(Error.RoleCannotContainParameterizedConstructor(method.DeclaringType, method));
          return;
        }

        bool remove = !method.RemainsInRoleInterface();
        if (remove) {
          Defer(() => {
            method.DeclaringType.Methods.Remove(method);
          });
          return;
        }

        if (method.IsFamily || method.IsFamilyOrAssembly) {
          // if the method is protected, mark it as guarded
          method.MarkAsGuarded(method.DeclaringType.Module);
        }

        method.Attributes = 
          MethodAttributes.Public | 
          MethodAttributes.HideBySig | // TODO: what about HideByName?
          MethodAttributes.NewSlot |
          MethodAttributes.Abstract |
          //MethodAttributes.Strict | // TODO: VB.NET uses this!
          MethodAttributes.Virtual;
        if (method.IsPropertyAccessor() || method.IsEventAccessor()) {
          method.Attributes |= MethodAttributes.SpecialName;
        }

        // the method body will be cleared in a wrap-up action
        Defer(() => {
          method.Body = null;
        });
      }

      #endregion

    }
  }

}
