using System;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Represents a member of a type.
  /// </summary>
  public abstract class TypeMember {

    /// <summary>
    /// The type the member belongs to.
    /// </summary>
    public TypeReference Type { get; private set; }

    /// <summary>
    /// The member definition.
    /// </summary>
    public IMemberDefinition Definition { get; private set; }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="class">The member type.</param>
    /// <param name="definition">The member definition.</param>
    public TypeMember(TypeReference type, IMemberDefinition definition) {
      if (type == null) throw new ArgumentNullException("type");
      if (definition == null) throw new ArgumentNullException("definition");
      Type = type;
      Definition = definition;
    }

  }

}
