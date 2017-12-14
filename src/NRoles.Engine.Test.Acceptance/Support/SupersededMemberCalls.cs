using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support.SupersededCalls {

  public abstract class Role_For_Super_Calls : Role {
    public virtual int Method() {
      return 42;
    }
  }

  [CompositionTest(
    Description = "Super call in composition should call superseded role method.",
    RoleType = typeof(Role_For_Super_Calls),
    TestType = typeof(Calls_Superseded_Method_Test),
    Ignore = true)]
  public class Calls_Superseded_Method_Composition : Does<Role_For_Super_Calls> {
    [Supersede] public int Method() {
      return this.Super<Role_For_Super_Calls>().Method();
      /*
        ldarg.0
        call !!0 NRoles.Ex::Super<class NRoles.Engine.Test.Support.SupersededCalls.Role_For_Super_Calls>(class [NRoles]NRoles.Does`1<!!0>)
        callvirt instance int32 NRoles.Engine.Test.Support.SupersededCalls.Role_For_Super_Calls::Method()
      */
    }
  }
  public class Calls_Superseded_Method_Test : DynamicTestFixture {
    public override void Test() {
      var c = new Calls_Superseded_Method_Composition();
      var r = c.Method();
      Assert.AreEqual(42, r);
    }
  }

  // TODO: method with parameters, expressions as arguments,
  //   properties, events, fields,
  //   abstract members

}

namespace NRoles {

  // TODO: migrate
  public static class Ex {
    public static TRole Super<TRole>(this Does<TRole> self) where TRole : Role {
      throw new InvalidOperationException();
    }
  }

}
