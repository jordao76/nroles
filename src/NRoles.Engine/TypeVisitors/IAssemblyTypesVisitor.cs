using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public interface IAssemblyTypesVisitor {
    void Visit(AssemblyDefinition assembly);
    void Visit(TypeDefinition typeDefinition);
  }

}
