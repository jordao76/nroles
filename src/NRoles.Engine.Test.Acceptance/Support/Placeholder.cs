using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support.PlaceholderMethods {

  [RoleTest(
    Description = "Placeholder in composition method should be replaced by concrete role method",
    CompositionType = typeof(DeepThought),
    TestType = typeof(DeepThoughtTest))]

  public class RDeepThought : Role {
    public int Answer() { return 42; }
  }

  public class DeepThought : Does<RDeepThought> {
    [Placeholder] public int Answer() { return 4 + 2; }
  }

  public class DeepThoughtTest : DynamicTestFixture {
    public override void Test() {
      var thinker = new DeepThought();
      Assert.AreEqual(42, thinker.Answer());
    }
  }

  // TODO: placeholder in abstract method
}
