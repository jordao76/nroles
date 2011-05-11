using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  public interface IAssemblyTypesVisitorRegistry {
    void Register(IAssemblyTypesVisitor visitor);
  }

}
