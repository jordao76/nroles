using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace NRoles.Engine {

  class MemberReaderVisitor : TypeVisitorBase {

    public List<IMemberDefinition> Members { get; private set; }

    public MemberReaderVisitor() {
      Members = new List<IMemberDefinition>();
    }

    public override void Visit(Collection<EventDefinition> eventDefinitionCollection) {
      foreach (var eventDefinition in eventDefinitionCollection) {
        Members.Add(eventDefinition);
      }
    }
    public override void Visit(Collection<PropertyDefinition> propertyDefinitionCollection) {
      foreach (var propertyDefinition in propertyDefinitionCollection) {
        Members.Add(propertyDefinition);
      }
    }
    public override void Visit(Collection<MethodDefinition> methodDefinitionCollection) {
      foreach (var methodDefinition in methodDefinitionCollection) {
        Members.Add(methodDefinition);
      }
    }
    public override void Visit(Collection<FieldDefinition> fieldDefinitionCollection) {
      foreach (var fieldDefinition in fieldDefinitionCollection) {
        Members.Add(fieldDefinition);
      }
    }

    public override void Visit(Collection<TypeDefinition> nestedTypeCollection) {
      // TODO??
    }

    public override void Visit(TypeDefinition typeDefinition) {
      if (typeDefinition == null) throw new ArgumentNullException("typeDefinition");
    }

  }
}
