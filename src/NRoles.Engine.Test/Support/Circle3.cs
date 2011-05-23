using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  //[RoleTest(
    //OtherRoles = new Type[] { typeof(RMagnitude<>) },
    //CompositionType = typeof(Circle3))]
  public abstract class REquality<T> : IEquatable<T>, Role {
    public abstract bool Equals(T other);
    public bool Differs(T other) { return !Equals(other); }
  }
  public abstract class RMagnitude<T> : Does<REquality<T>>, Role {
    public abstract bool Smaller(T other);
    public bool Between(T min, T max) {
      return this.Greater(min) && this.Smaller(max);
    }
    public bool Greater(T other) {
      return !Smaller(other) && this.As<REquality<T>>().Differs(other);
    }
  }
  public class Circle3 : Does<REquality<Circle3>> /*, Does<RMagnitude<Circle3>>*/ {
    // NOTE: this inheritance gave strange results:
    //   Does<REquality<Circle>>, Does<RMagnitude<Circle>>; and use Circle everywhere Circle3 is used below
    public int Center { get; set; }
    public int Radius { get; set; }
    public double Area { get { return Math.PI * (Radius * Radius); } }
    public bool Equals(Circle3 other) {
      if (other == null) return false;
      return other.Center == Center && other.Radius == Radius;
    }
    public bool Smaller(Circle3 other) {
      if (other == null) return false;
      return Radius < other.Radius;
    }
  }

}
