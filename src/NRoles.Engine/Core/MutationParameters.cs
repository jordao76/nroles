using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Parameters for a mutation operation.
  /// </summary>
  public class MutationParameters {

    // TODO: the context has roughly the same info as this class. Get rid of it?
    /// <summary>
    /// The mutation context.
    /// </summary>
    public MutationContext Context { get; set; }

    /// <summary>
    /// Assembly being mutated.
    /// </summary>
    public AssemblyDefinition Assembly { get { return Context.Assembly; } }

    /// <summary>
    /// Specific type being mutated. If it's null, the whole assembly will be mutated.
    /// </summary>
    public TypeDefinition SourceType { get; set; }

    /// <summary>
    /// Validates the parameters.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Assembly"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="SourceType"/> is not null and not in the <see cref="Context"/> assembly.</exception>
    public virtual void Validate() {
      if (Context == null) throw new InvalidOperationException("Context is null");
      if (Assembly == null) throw new InvalidOperationException("Assembly is null");
      if (SourceType != null && SourceType.Module.Assembly != Assembly) throw new InvalidOperationException("SourceType is not in the Context assembly");
    }

  }

}
