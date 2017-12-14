using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NRoles.Engine.Test;

namespace NRoles.Engine.Test.Support.PlaceholderEvents {

  [RoleTest(
    Description = "Placeholder in composition event should be replaced by concrete role event",
    CompositionType = typeof(DeepThought),
    TestType = typeof(DeepThoughtTest))]

  public class RDeepThought : Role {
    public event EventHandler<EventArgs<int>> Answer;
    public void Fire() {
      Answer(this, new EventArgs<int>(42));
    }
  }

  public class DeepThought : Does<RDeepThought> {
    [Placeholder] public event EventHandler<EventArgs<int>> Answer {
      add { } 
      remove { }
    }
    [Placeholder] public void Fire() {}
  }

  public class DeepThoughtTest : DynamicTestFixture {
    public override void Test() {
      var thinker = new DeepThought();
      var answer = 0;
      thinker.Answer += ((object source, EventArgs<int> ea) => answer = ea.Element);
      thinker.Fire();
      Assert.AreEqual(42, answer);
    }
  }

  // TODO: possible bug: test superseded event

  // TODO: placeholder in individual adder or remover

}
