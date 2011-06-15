using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support {

  #region Properties

  class Sensing { public string Value = "initial"; }
  [RoleTest(
    CompositionType = typeof(Role_With_Public_Property_Composition),
    TestType = typeof(Role_With_Public_Property_Composition_Test))]
  class Role_With_Public_Property : Role {
    public Sensing Property { get { return new Sensing(); } set { value.Value = "set!"; } }
  }
  class Role_With_Public_Property_Composition : Does<Role_With_Public_Property> { }
  class Role_With_Public_Property_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Public_Property_Composition().As<Role_With_Public_Property>();
      Assert.AreEqual("initial", role.Property.Value);
      var sensing = new Sensing();
      role.Property = sensing;
      Assert.AreEqual("set!", sensing.Value);
    }
  }

  [RoleTest(
    CompositionType = typeof(Role_With_Public_Auto_Property_Composition),
    TestType = typeof(Role_With_Public_Auto_Property_Composition_Test))]
  class Role_With_Public_Auto_Property : Role {
    public string Property { get; set; }
  }
  class Role_With_Public_Auto_Property_Composition : Does<Role_With_Public_Auto_Property> { }
  class Role_With_Public_Auto_Property_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Public_Auto_Property_Composition().As<Role_With_Public_Auto_Property>();
      Assert.IsNull(role.Property);
      role.Property = "test";
      Assert.AreEqual("test", role.Property);
    }
  }

  [RoleTest(
    CompositionType = typeof(Role_With_Private_Property_Composition),
    TestType = typeof(Role_With_Private_Property_Composition_Test))]
  class Role_With_Private_Property : Role {
    private string Property { get; set; }
    public string PublicProperty {
      get { return Property; }
      set { Property = value; }
    }
  }
  class Role_With_Private_Property_Composition : Does<Role_With_Private_Property> { }
  class Role_With_Private_Property_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Private_Property_Composition().As<Role_With_Private_Property>();
      Assert.IsNull(role.PublicProperty);
      role.PublicProperty = "test";
      Assert.AreEqual("test", role.PublicProperty);
    }
  }

  #endregion

}
