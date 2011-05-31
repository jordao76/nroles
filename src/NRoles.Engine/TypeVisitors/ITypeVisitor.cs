using Mono.Cecil;
using Mono.Collections.Generic;

namespace NRoles.Engine {

  /// <summary>
  /// Visitor for a type definition.
  /// </summary>
  public interface ITypeVisitor {

    /// <summary>
    /// Visits the type definition. This is the first method called when visiting a type.
    /// </summary>
    /// <param name="typeDefinition">Represents the type to visit.</param>
    void Visit(TypeDefinition typeDefinition);

    /// <summary>
    /// Visits the security declarations of a type.
    /// </summary>
    /// <param name="securityDeclarationCollection">Security declarations.</param>
    void Visit(Collection<SecurityDeclaration> securityDeclarationCollection);

    /// <summary>
    /// Visits the generic parameters of a type.
    /// </summary>
    /// <param name="genericParameterCollection">Generic parameters.</param>
    void Visit(Collection<GenericParameter> genericParameterCollection);
    
    /// <summary>
    /// Visits the declared interfaces of a type.
    /// </summary>
    /// <param name="interfaceCollection">Interfaces.</param>
    void Visit(Collection<TypeReference> interfaceCollection);
    
    /// <summary>
    /// Visits the custom attributes of a type.
    /// </summary>
    /// <param name="customAttributeCollection">Custom attributes.</param>
    void Visit(Collection<CustomAttribute> customAttributeCollection);
    
    /// <summary>
    /// Visits the events of a type.
    /// </summary>
    /// <param name="eventDefinitionCollection">Events.</param>
    void Visit(Collection<EventDefinition> eventDefinitionCollection);
    
    /// <summary>
    /// Visits the fields of a type.
    /// </summary>
    /// <param name="fieldDefinitionCollection">Fields.</param>
    void Visit(Collection<FieldDefinition> fieldDefinitionCollection);
    
    /// <summary>
    /// Visits the properties of a type.
    /// </summary>
    /// <param name="propertyDefinitionCollection">Properties.</param>
    void Visit(Collection<PropertyDefinition> propertyDefinitionCollection);
    
    /// <summary>
    /// Visits the methods of a type.
    /// </summary>
    /// <param name="methodDefinitionCollection">Methods.</param>
    void Visit(Collection<MethodDefinition> methodDefinitionCollection);
    
    /// <summary>
    /// Visits the nested types of a type.
    /// </summary>
    /// <param name="nestedTypeCollection">Nested types.</param>
    void Visit(Collection<TypeDefinition> nestedTypeCollection);
  }

  /// <summary>
  /// Base class for a type visitor. Declares all methods as virtual with empty bodies.
  /// </summary>
  public abstract class TypeVisitorBase : ITypeVisitor {
    public virtual void Visit(TypeDefinition typeDefinition) { }
    public virtual void Visit(Collection<SecurityDeclaration> securityDeclarationCollection) { }
    public virtual void Visit(Collection<GenericParameter> genericParameterCollection) { }
    public virtual void Visit(Collection<TypeReference> interfaceCollection) { }
    public virtual void Visit(Collection<CustomAttribute> customAttributeCollection) { }
    public virtual void Visit(Collection<EventDefinition> eventDefinitionCollection) { }
    public virtual void Visit(Collection<FieldDefinition> fieldDefinitionCollection) { }
    public virtual void Visit(Collection<PropertyDefinition> propertyDefinitionCollection) { }
    public virtual void Visit(Collection<MethodDefinition> methodDefinitionCollection) { }
    public virtual void Visit(Collection<TypeDefinition> nestedTypeCollection) { }
  }

}
