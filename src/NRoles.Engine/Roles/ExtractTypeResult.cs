using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public class ExtractTypeResult : OperationResult {
    public TypeDefinition TargetType { get; internal set; }
  }

}
