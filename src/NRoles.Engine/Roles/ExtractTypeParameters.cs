using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  public class ExtractTypeParameters : MutationParameters {
    public ExtractTypeParameters() {
      AddTypeStrategy = AddTypeStrategy.DontAdd;
    }
    public string TargetTypeName { get; set; }
    public AddTypeStrategy AddTypeStrategy { get; set; }
    public override void Validate() {
      base.Validate();
      if (SourceType == null) throw new InvalidOperationException("SourceType is null");
      if (TargetTypeName == null) throw new InvalidOperationException("TargetTypeName is null");
      TargetTypeName = TargetTypeName.Trim();
      if (TargetTypeName.Length == 0) throw new InvalidOperationException("TargetTypeName is empty");
    }
  }

  /// <summary>
  /// Strategy to add a generated type to an assembly.
  /// </summary>
  public enum AddTypeStrategy {

    /// <summary>
    /// Don't add the type to the assembly.
    /// </summary>
    DontAdd,

    /// <summary>
    /// Add the type as a nested type to the source type.
    /// </summary>
    AddAsNested

  }

}
