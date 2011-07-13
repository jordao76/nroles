using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Represents a member of a composition class (not from its roles).
  /// </summary>
  public class ClassMember : RoleCompositionMember {

    /// <summary>
    /// The class the member belongs to.
    /// </summary>
    public TypeReference Class { get { return Type; } }

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
    /// Indicates if the represented member is inherited.
    /// </summary>
    /// <remarks>
    /// If it's an inherited member, the <see cref="Class"/> property will be the 
    /// parent class where the member is defined.
    /// </remarks>
    public bool IsInherited { get; private set; }

    /// <summary>
    /// Indicates if the member is a placeholder.
    /// A placeholder member is meant to be replaced.
    /// </summary>
    public bool IsPlaceholder {
      get { return Definition.IsPlaceholder(); }
    }

    /// <summary>
    /// Indicates if the member is abstract.
    /// </summary>
    public override bool IsAbstract {
      get {
        // abstractedness is only applicable to method definitions
        return
          Definition is MethodDefinition &&
          ((MethodDefinition)Definition).IsAbstract;
      }
    }

    /// <summary>
    /// A foreign member is a member that comes from outside the <see cref="Type"/>.
    /// <see cref="ClassMember"/>s are never foreign.
    /// </summary>
    public override bool IsForeign { 
      get { return false; } 
    }

    public override void Process(MemberConflictResolver resolver) {
      resolver.Process(this);
    }

    // TODO: the next 2 methods are not relevant for this class

    public override RoleCompositionMember ResolveImplementingMember() {
      return this; // TODO: what if it's a placeholder?
    }

    public override IEnumerable<RoleCompositionMember> ResolveOverridingMembers() {
      return new RoleCompositionMember[] { };
    }

  }

}
