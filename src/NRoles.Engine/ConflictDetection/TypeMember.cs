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

    /// <summary>
    /// Resolves the definition of the encapsulated member in the context of 
    /// the member's class. This will only be different from the direct class
    /// member if the class is generic and type arguments are provided for it.
    /// So, e.g., <c>void Add(T p)</c> in <c>List&lt;T&gt;</c> will become
    /// <c>void Add(int p)</c> if the actual class is <c>List&lt;int&gt;</c>.
    /// </summary>
    public IMemberDefinition ResolveContextualDefinition() {
      return new MemberResolver(Type).ResolveMemberDefinition(Definition);
    }

  }

}
