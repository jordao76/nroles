using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support.AbstractMembers {

  public abstract class Base {
    public abstract int Method();
  }

  [CompositionTest(
    Description = "Abstract method in a base class should be 'superseded' by abstract role method",
    RoleType = typeof(Role_With_Abstract_Method),
    TestType = typeof(Child_Test))]
  public abstract class Derived : Base, Does<Role_With_Abstract_Method> { }

  public class Child : Derived {
    public override int Method() {
      return 1;
    }
  }

  public abstract class Role_With_Abstract_Method : Role {
    public abstract int Method();
    public virtual int Answer {
      get { return Method() + 41; }
    }
  }

  public class Child_Test : DynamicTestFixture {
    public override void Test() {
      
      //var r = new Child().As<Role_With_Abstract_Method>(); // mono got confused with the extension method here
      var r = NRoles.Roles.As<Role_With_Abstract_Method>(new Child());
      Assert.AreEqual(42, r.Answer);
    }
  }

}
