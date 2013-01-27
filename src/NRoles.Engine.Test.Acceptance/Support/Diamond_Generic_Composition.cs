using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support {

  [RoleTest(
    OtherRoles = new Type[] { typeof(Diamond_Left_Role<>), typeof(Diamond_Right_Role<>) },
    CompositionType = typeof(Diamond_Aliased_Composition),
    TestType = typeof(Diamond_Aliased_Composition_Test))]
  public abstract class Diamond_Base_Role<S> : Role where S : Does<Diamond_Base_Role<S>> {
    public abstract int M(S s);
  }
  public class Diamond_Left_Role<S> : Role, Does<Diamond_Base_Role<S>> where S : Does<Diamond_Base_Role<S>>, Does<Diamond_Left_Role<S>> {
    public int M(S s) {
      return 40;
    }
  }
  public class Diamond_Right_Role<S> : Role, Does<Diamond_Base_Role<S>> where S : Does<Diamond_Base_Role<S>>, Does<Diamond_Right_Role<S>> {
    public int M(S s) {
      return 2;
    }
  }
  interface Left_View : RoleView<Diamond_Left_Role<Diamond_Aliased_Composition>> { 
    [Aliasing("M")] int LeftM(Diamond_Aliased_Composition s);
  }
  interface Right_View : RoleView<Diamond_Right_Role<Diamond_Aliased_Composition>> {
    [Aliasing("M")] int RightM(Diamond_Aliased_Composition s);
  }
  public class Diamond_Aliased_Composition : 
    Does<Diamond_Left_Role<Diamond_Aliased_Composition>>, 
    Does<Diamond_Right_Role<Diamond_Aliased_Composition>>,
    Does<Diamond_Base_Role<Diamond_Aliased_Composition>>,
    Does<Left_View>, Does<Right_View>
  {
    public int M(Diamond_Aliased_Composition s) {
      return this.As<Left_View>().LeftM(null) + this.As<Right_View>().RightM(null); 
    }
  }
  public class Diamond_Aliased_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var c = new Diamond_Aliased_Composition();
      Assert.AreEqual(42, c.M(null));
      Assert.AreEqual(42, c.As<Diamond_Base_Role<Diamond_Aliased_Composition>>().M(null));
      Assert.AreEqual(40, c.As<Diamond_Left_Role<Diamond_Aliased_Composition>>().M(null));
      Assert.AreEqual(2, c.As<Diamond_Right_Role<Diamond_Aliased_Composition>>().M(null));
      Assert.AreEqual(40, c.As<Left_View>().LeftM(null));
      Assert.AreEqual(2, c.As<Right_View>().RightM(null));
    }
  }
}
