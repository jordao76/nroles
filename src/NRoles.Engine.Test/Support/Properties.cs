using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support {

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

  [RoleTest(CompositionType = typeof(Role_With_Property_Composition_With_Property_Superceded))]
  [RoleTest(CompositionType = typeof(Derived_Class_From_Base_Class_With_Property))]
  public interface Role_With_Property : Role {
    int Property { get; }
  }
  public class Role_With_Property_Composition_With_Property_Superceded : Does<Role_With_Property> {
    public int Property { get; set; }
  }
  public class Base_Class_With_Property {
    public int Property { get { return 42; } }
  }
  public class Derived_Class_From_Base_Class_With_Property : Base_Class_With_Property, Does<Role_With_Property> { }
  // TODO: finish this test

}
