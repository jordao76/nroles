using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  public class Simple_Role_With_Method : Role {
    public int Method() { return 42; }
  }

  public class Simple_Role_With_Property : Role {
    public int Property { get; set; }
  }

  public class Role_With_Many_Members : Role {
    public int Property { get; set; }
    public event EventHandler Event;
    public void Method() { Event(this, EventArgs.Empty); }
    public string this[int x, int y] {
      get { return "Test"; }
      set { }
    }
    public string Field;
  }

  public class Complicated_Role<T, U, V, W, X, Y, Z> : Role
    where Z : EventArgs
    where T : IComparable<U>
    where W : class, new()
    where X : struct {
    public X PublicMethodExtraTypeParameters<A, B>(T p1, out U p2, ref A p3, ICollection<T> ts, params B[] p4)
    where B : struct {
      p2 = default(U);
      return default(X);
    }
    public X PublicMethod(T p1, out U p2, ref W p3, ICollection<U> us, params Y[] p4) { // no extra type parameters
      p2 = default(U);
      return default(X);
    }
    public Y PublicAutoProperty { get; set; }
    public W this[X x, Y y] { get { return new W(); } set { } } // indexer
    public delegate A PublicDelegate<A>(T p1, out U p2, ref V p3, params W[] p4);
    public class Public_Nested_Generic_Class<A, B> {
      public void PublicNestedMethod(T t, A a) { }
    }
  }
  
  public class Role_That_Uses_Complicated_Role : Role, Does<Complicated_Role<int, int, int, object, int, int, EventArgs>> { }

  public class Complicated_Role_Composition : Does<Complicated_Role<int, int, int, object, int, int, EventArgs>> { }
  
}
