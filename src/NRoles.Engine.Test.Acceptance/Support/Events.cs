using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support {

  [RoleTest(
    Ignore = true,
    CompositionType = typeof(Role_With_Public_Event_Composition),
    TestType = typeof(Role_With_Public_Event_Test))]
  public class Role_With_Public_Event : Role {
    public event EventHandler MyEvent = delegate { };
    public void Operation() { FireEvent(); }
    private void FireEvent() { MyEvent(this, EventArgs.Empty); }
  }
  public class Role_With_Public_Event_Composition : Does<Role_With_Public_Event> { }
  public class Role_With_Public_Event_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Public_Event_Composition().As<Role_With_Public_Event>();
      string str = null;
      var handler = (EventHandler)((s, e) => str = "Hello");
      role.MyEvent += handler;
      Assert.IsNull(str);
      role.Operation();
      Assert.AreEqual("Hello", str);
      str = "Bye";
      role.MyEvent -= handler;
      role.Operation();
      Assert.AreEqual("Bye", str);
    }
  }

  public class EventArgs<T> : EventArgs {
    public readonly T Element;
    public EventArgs(T t) { Element = t; }
  }

  [RoleTest(
    Ignore = true,
    CompositionType = typeof(Generic_Role_With_Event_Composition<>),
    TestType = typeof(Generic_Role_With_Event_Test))]
  public class Generic_Role_With_Event<T> : Role {
    public event EventHandler<EventArgs<T>> MyEvent = delegate { };
    public void Operation(T element) { FireEvent(element); }
    private void FireEvent(T element) { MyEvent(this, new EventArgs<T>(element)); }
  }
  public class Generic_Role_With_Event_Composition<T> : Does<Generic_Role_With_Event<T>> { }
  public class Generic_Role_With_Event_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Generic_Role_With_Event_Composition<int>().As<Generic_Role_With_Event<int>>();
      string str = null;
      var handler = (EventHandler<EventArgs<int>>)((s, e) => {
        Assert.AreEqual(42, e.Element);
        str = "Hello";
      });
      role.MyEvent += handler;
      role.Operation(42);
      Assert.AreEqual("Hello", str);
      str = "Bye";
      role.MyEvent -= handler;
      role.Operation(42);
      Assert.AreEqual("Bye", str);
    }
  }

  [RoleTest(
    Ignore = true,
    CompositionType = typeof(Role_With_Internal_Event_Composition))]
  public class Role_With_Internal_Event : Role {
    internal event EventHandler MyEvent = delegate { };
    public void Operation() { FireEvent(); }
    private void FireEvent() { MyEvent(this, EventArgs.Empty); }
  }
  public class Role_With_Internal_Event_Composition : Does<Role_With_Internal_Event> { }

}
