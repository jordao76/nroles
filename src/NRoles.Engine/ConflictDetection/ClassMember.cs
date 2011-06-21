using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Represents a member of a class (or interface).
  /// </summary>
  public class ClassMember : TypeMember {

    /// <summary>
    /// The class the member belongs to.
    /// </summary>
    public TypeReference Class { get { return Type; } }
    
    /// <summary>
    /// Indicates if the represented member is inherited.
    /// </summary>
    /// <remarks>
    /// If it's an inherited member, the <see cref="Class"/> property will be the 
    /// parent class where the member is defined.
    /// </remarks>
    public bool IsInherited { get; private set; }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="class">The member class.</param>
    /// <param name="definition">The member definition.</param>
    /// <param name="isInherited">If the member is inherited.</param>
    public ClassMember(TypeReference @class, IMemberDefinition definition, bool isInherited = false) :
      base(@class, definition)
    {
      IsInherited = isInherited;
    }

    /// <summary>
    /// Indicates if the member is a placeholder.
    /// A placeholder member is meant to be replaced.
    /// </summary>
    public bool IsPlaceholder {
      get {
        return Definition.IsPlaceholder();
      }
    }
  }

}
