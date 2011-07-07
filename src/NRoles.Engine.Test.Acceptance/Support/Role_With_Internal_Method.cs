using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace NRoles.Engine.Test.Support {
  
  [RoleTest(
    CompositionType = typeof(Role_With_Internal_Method_Composition),
    TestType = typeof(Role_With_Internal_Method_Composition_Test))]
  public class Role_With_Internal_Method : Role {
    public int PublicMethod() {
      return 2 + InternalMethod();
    }
    internal int InternalMethod() {
      return 42;
    }
  }
  public class Role_With_Internal_Method_Composition : Does<Role_With_Internal_Method> { }
  public class Role_With_Internal_Method_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Internal_Method_Composition().As<Role_With_Internal_Method>();
      Assert.AreEqual(44, role.PublicMethod());
      Assert.AreEqual(42, role.InternalMethod());
    }
    public void Ignored() {
      var role = new Role_With_Internal_Method_Composition().As<Role_With_Internal_Method>();
      // this call should be adjusted and it should not fail PEVerify
      role.InternalMethod();
    }
  }

}
