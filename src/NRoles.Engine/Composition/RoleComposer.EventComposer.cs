using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  partial class RoleComposer {

    sealed partial class MemberComposer : IMessageContainer {

      private EventDefinition RoleEvent { get { return (EventDefinition)_roleMember.Definition; } }

      private EventDefinition ImplementEvent() {
        Tracer.TraceVerbose("Compose event: {0}", _name);

        var placeholder = ((EventDefinition)Group.Placeholder);

        var implementedEvent = 
          placeholder ??
          new MemberResolver(Role, Module).ResolveEventDefinition(RoleEvent);

        if (placeholder == null) {
          TargetType.Events.Add(implementedEvent);
        }

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
