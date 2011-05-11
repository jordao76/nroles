using System;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Uses a type visitor to visit a type definition.
  /// </summary>
  /// <seealso cref="ITypeVisitor"/>
  public class TypeCrawler {

    private readonly TypeDefinition _type;

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="typeDefinition">Type definition to visit.</param>
    public TypeCrawler(TypeDefinition typeDefinition) {
      if (typeDefinition == null) throw new ArgumentNullException("typeDefinition");
      _type = typeDefinition;
    }

    /// <summary>
    /// Calls the given visitor for the type's component.
    /// </summary>
    /// <param name="visitor">Visitor to use.</param>
    public void Accept(ITypeVisitor visitor) {
      if (visitor == null) throw new ArgumentNullException("visitor");

      visitor.Visit(_type);
      if (_type.HasSecurityDeclarations) visitor.Visit(_type.SecurityDeclarations);
      if (_type.HasGenericParameters) visitor.Visit(_type.GenericParameters);
      if (_type.HasInterfaces) visitor.Visit(_type.Interfaces);
      if (_type.HasCustomAttributes) visitor.Visit(_type.CustomAttributes);
      if (_type.HasEvents) visitor.Visit(_type.Events);
      if (_type.HasFields) visitor.Visit(_type.Fields);
      if (_type.HasProperties) visitor.Visit(_type.Properties);
      if (_type.HasMethods) visitor.Visit(_type.Methods);
      if (_type.HasNestedTypes) visitor.Visit(_type.NestedTypes);
    }

  }

  /// <summary>
  /// Type definition extensions for visitors.
  /// </summary>
  public static class TypeDefinitionCrawlerExtension {

    /// <summary>
    /// Extension method that uses the <see cref="TypeCrawler"/> to visit a type with a visitor.
    /// </summary>
    /// <param name="self">Instance parameter. Type to be visited.</param>
    /// <param name="visitor">The visitor to use.</param>
    public static void Accept(this TypeDefinition self, ITypeVisitor visitor) {
      if (self == null) throw new InstanceArgumentNullException();
      new TypeCrawler(self).Accept(visitor);
    }
  }

}
