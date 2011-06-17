using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support {

  [RoleTest(
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

  // TODO: properties and events

  // TODO: think about placeholders in roles that are also compositions

  // TODO: placeholder in abstract method

  // TODO: placeholder in an override of a base-class member

}
