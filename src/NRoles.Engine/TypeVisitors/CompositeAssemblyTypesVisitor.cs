using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace NRoles.Engine {

  class CompositeAssemblyTypesVisitor : IAssemblyTypesVisitor, IAssemblyTypesVisitorRegistry {

    private List<IAssemblyTypesVisitor> _visitors = new List<IAssemblyTypesVisitor>();

    public void Register(IAssemblyTypesVisitor visitor) {
      if (visitor == null) throw new ArgumentNullException("visitor");
      _visitors.Add(visitor);
    }

    public void Visit(AssemblyDefinition assembly) {
      _visitors.ForEach(v => v.Visit(assembly));
      // this composite drill down, so it should not be composed with itself
      assembly.MainModule.GetAllTypes().ForEach(td => Visit(td));
    }

    public void Visit(TypeDefinition typeDefinition) {
      _visitors.ForEach(v => v.Visit(typeDefinition));
    }
  }
}