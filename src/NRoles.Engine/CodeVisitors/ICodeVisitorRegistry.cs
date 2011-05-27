using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public interface ICodeVisitorRegistry {
    void Register(CodeVisitorBase visitor);
    void Register(CodeVisitorBase visitor, TypeDefinition scopedAtType);
  }

}
