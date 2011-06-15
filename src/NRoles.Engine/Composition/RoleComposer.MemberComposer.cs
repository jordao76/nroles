using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  partial class RoleComposer {

    sealed partial class MemberComposer : IMessageContainer {

      List<Message> _messages = new List<Message>();
      public IEnumerable<Message> Messages { get { return _messages; } }
      public void AddMessage(Message message) { 
        _messages.Add(message);
      }

      public readonly TypeDefinition TargetType;
      public readonly RoleCompositionMemberContainer Container;
      public ModuleDefinition Module { get { return TargetType.Module; } }

      public MemberComposer(TypeDefinition targetType, RoleCompositionMemberContainer container) {
        TargetType = targetType;
        Container = container;
      }

      private string _name;
      private RoleCompositionMember _roleMember;
      private IEnumerable<RoleCompositionMember> _overrides;
      private MethodAttributes _accessSpecifier;
      private TypeReference Role { get { return _roleMember.Role; } }

      public IMemberDefinition Compose(ContributedConflictGroup group) {
        return Compose(group, MethodAttributes.Public);
      }

      public IMemberDefinition Compose(ContributedConflictGroup group, MethodAttributes accessSpecifier) {
        if (group.ImplementedMember != null) {
          return group.ImplementedMember;
        }
        if (group.DontImplement) {
          return null;
        }

        if (group.IsSuperceded) {
          // the superceding member in the class is the implementing member for the role member
          return group.ImplementedMember = AdjustSupercedingMember(group.Supercede, group.ResolveOverridingMembers());
        }

        if (group.IsBaseMethod) {
          ImplementBaseMethod(group.Members[0], group.ResolveOverridingMembers()); // any member will do, they all point to the same base method
          // TODO: use the ImplementedMember here?
          return null;
        }

        // the resolved member implements all the other members in the group by calling its implementing member
        var resolvedMember = group.ResolvedMember;

        if (resolvedMember == null) {
          // this member does not get composed (it will be aliased by another member)
          return null;
        }

        var implementedMember = ImplementMember(
          resolvedMember.Definition.Name, 
          resolvedMember.ResolveImplementingMember(), 
          group.ResolveOverridingMembers(),
          accessSpecifier);
        if (implementedMember == null) {
          // this member was not implemented (it was provided by a base class)
          return null;
        }
        if (group.ReuseSlot) {
          ((MethodDefinition)implementedMember).IsNewSlot = false;
        }
        return group.ImplementedMember = implementedMember;
      }

      private IMemberDefinition ImplementMember(string name, RoleCompositionMember roleMember, IEnumerable<RoleCompositionMember> overrides, MethodAttributes accessSpecifier) {
        _name = name;
        _roleMember = roleMember;
        _overrides = overrides;
        _accessSpecifier = accessSpecifier;
        return ImplementMember();
      }

      private IMemberDefinition ImplementMember() {
        var memberType = _roleMember.Definition.GetType();

        if (_roleMember.Definition.IsGuarded(TargetType.Module)) {
          _accessSpecifier = MethodAttributes.Family;
        }
        if (_roleMember.Definition.IsHidden(TargetType.Module)) {
          _accessSpecifier = MethodAttributes.Private;
        }

        if (memberType == typeof(MethodDefinition)) {
          if (RoleMethod.IsPropertyAccessor()) {
            return ImplementPropertyAccessor();
          }
          // TODO: if it's an event accessor: ??
          else {
            if (_backingField != null) throw new InvalidOperationException("The member is a method BUT has a backing field!"); // TODO: assertion!
            return ImplementMethodCallingCodeClass();
          }
        }

        if (memberType == typeof(PropertyDefinition)) {

          if (IsStateClassProperty(RoleProperty)) {
            // only implement this property if it's not already implemented by a base class
            if (!IsRoleAlreadyComposedByAncestors(Role, TargetType)) {
              return ImplementPropertyWithBackingField();
            }
            else {
              DontImplementPropertyAccessorMethods();
              return null;
            }
          }
          else {
            return ImplementPropertyCallingCodeClass();
          }

        }

        if (memberType == typeof(EventDefinition)) {
          return ImplementEvent();
        }

        throw new InvalidOperationException(
          string.Format("Can't implement member '{0}' of type '{1}'", _roleMember.Definition.Name, memberType.Name));
      }

      private bool IsRoleAlreadyComposedByAncestors(TypeReference role, TypeDefinition targetType) {
        // TODO: it might make sense to do this logic on the conflict resolution phase!
        var currentType = targetType.BaseType; // start the search at the base type
        do {
          if (currentType.RetrieveRoles().Contains(role)) {
            return true;
          }
          currentType = currentType.Resolve().BaseType;
        } while (currentType != null);
        return false;
      }

      private MethodDefinition AdjustSupercedingMember(ClassMember classMember, IEnumerable<RoleCompositionMember> overrides) {
        Tracer.TraceVerbose("Adjust superceding member: {0}", classMember.Definition);
        
        var member = classMember.Definition;
        var method = member as MethodDefinition;
        if (method == null) return null;

        MethodDefinition targetMethod = null;
        if (!classMember.IsInherited) {
          targetMethod = method;
        }
        else {
          // if it's in a base class, create a new method in the target class that calls the base class method
          targetMethod = new MemberResolver(classMember.Class, Module).ResolveMethodDefinition(method);
          if (method.IsVirtual && !method.IsFinal) {
            targetMethod.IsNewSlot = false; // the derived method overrides the base method
          }
          CreateCodeToCallBaseClassMethod(targetMethod, classMember);
          TargetType.Methods.Add(targetMethod);
        }

        // add the corresponding overrides to the method
        AddOverrides(targetMethod, overrides);

        if (!(method.IsVirtual || method.IsAbstract)) {
          // to support polymorphism with regards to the role interface, mark as virtual sealed
          targetMethod.Attributes |= MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final;
        }

        return targetMethod;
     }

      private void ImplementBaseMethod(RoleCompositionMember typeMember, IEnumerable<RoleCompositionMember> overrides) {
        Tracer.TraceVerbose("Implement base method: {0}", typeMember.Definition);

        string baseMethodName = NameProvider.GetOriginalBaseMethodName(typeMember.Definition.Name);
        ClassMember baseMember = null;
        var currentType = TargetType.BaseType;
        do {
          var finder = new MemberFinder(currentType.Resolve());
          var foundBase = finder.FindMatchFor(typeMember.Definition, baseMethodName);
          if (foundBase != null) {
            baseMember = new ClassMember(currentType, foundBase, isInherited: true);
            break;
          }
          currentType = currentType.Resolve().BaseType;
        } while (currentType != null);
        if (baseMember == null) throw new InvalidOperationException();

        // TODO: refactor with AdjustSupercedingMember!
        var method = (MethodDefinition)typeMember.Definition;
        var targetMethod = new MemberResolver(baseMember.Class, Module).ResolveMethodDefinition(method, method.Name, MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig);
        CreateCodeToCallBaseClassMethod(targetMethod, baseMember);
        TargetType.Methods.Add(targetMethod);
        AddOverrides(targetMethod, overrides);
      }

      private void CreateCodeToCallBaseClassMethod(MethodDefinition derivedMethod, ClassMember baseMember) {
        var baseMethod = (MethodDefinition)baseMember.Definition;
        var methodReference = TargetType.Module.Import(baseMethod); // need to Import a method from another assembly. TODO: move this code to the MemberResolver, make it use the MutationContext
        PushParameters(derivedMethod);
        EmitCodeToCallMethod(derivedMethod, methodReference);
        derivedMethod.Body.GetILProcessor().Emit(OpCodes.Ret);
      }

      // TODO: extract to its own partial class (RoleComposer.EventComposer.cs)

      private EventDefinition RoleEvent { get { return (EventDefinition)_roleMember.Definition; } }

      private EventDefinition ImplementEvent() {
        Tracer.TraceVerbose("Compose event: {0}", _name);

        var implementedEvent = new MemberResolver(Role, Module).ResolveEventDefinition(RoleEvent);
        TargetType.Events.Add(implementedEvent);

        var eventAccessorComposer = new MemberComposer(TargetType, Container);
        ImplementEventAccessorMethods(implementedEvent, eventAccessorComposer);

        return implementedEvent;
      }

      private void ImplementEventAccessorMethods(EventDefinition implementedEvent, MemberComposer propertyAccessorComposer) {
        if (RoleEvent.AddMethod != null) {
          var addMethodRoleGroup = Container.ResolveGroup(Role, RoleEvent.AddMethod);
          implementedEvent.AddMethod = (MethodDefinition)propertyAccessorComposer.Compose(addMethodRoleGroup, _accessSpecifier);
        }
        if (RoleEvent.RemoveMethod != null) {
          var removeMethodRoleGroup = Container.ResolveGroup(Role, RoleEvent.RemoveMethod);
          implementedEvent.RemoveMethod = (MethodDefinition)propertyAccessorComposer.Compose(removeMethodRoleGroup, _accessSpecifier);
        }
        if (RoleEvent.InvokeMethod != null) {
          var invokeMethodRoleGroup = Container.ResolveGroup(Role, RoleEvent.InvokeMethod);
          implementedEvent.RemoveMethod = (MethodDefinition)propertyAccessorComposer.Compose(invokeMethodRoleGroup, _accessSpecifier);
        }
      }

    }
  
  }
}
