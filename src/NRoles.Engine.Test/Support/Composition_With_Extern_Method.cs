using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  //[RoleTest(CompositionType = typeof(Composition_With_Extern_Method))]
  public class Simple_Role_For_Composition_With_Extern_Method : Role {
    public void Method() { }
  }
  public class Composition_With_Extern_Method : Does<Simple_Role_For_Composition_With_Extern_Method> {
    // TODO: use DllImport; without it NUnit can't load this assembly
    //[Placeholder] public extern void Extern_Method();
  }

  public class PlaceholderAttribute : Attribute {}
}
