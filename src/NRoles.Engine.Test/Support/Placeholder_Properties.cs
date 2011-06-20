using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support.PlaceholderProperties {

  [RoleTest(
    Description = "Placeholder in composition property should be replaced by concrete role property getter and setter",
    CompositionType = typeof(DeepThought),
    TestType = typeof(DeepThoughtTest))]

  public class RDeepThought : Role {
    private int answer;
    public int Answer {
      get { return answer + 2; }
      set { answer = value - 2; }
    }
  }

  public class DeepThought : Does<RDeepThought> {
    [Placeholder] public int Answer { 
      get { return 4 + 2; } 
      set { } 
    }
  }

  public class DeepThoughtTest : DynamicTestFixture {
    public override void Test() {
      var thinker = new DeepThought();
      thinker.Answer = 42;
      Assert.AreEqual(42, thinker.Answer);
    }
  }

  // TODO: possible bug: test superceded property

  // TODO: auto-property

  // TODO: placeholder in individual getter or setter

}
