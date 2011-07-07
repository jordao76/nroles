using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  [RoleTest(
    OtherRoles = new Type[] { typeof(RMagnitude) },
    CompositionType = typeof(Circle2))]
  public abstract class REquality : Role {
    public abstract bool Equals(REquality other);
    public bool Differs(REquality other) { return !Equals(other); }
  }
  public abstract class RMagnitude : Does<REquality>, Role {
    public abstract bool Smaller(RMagnitude other);
    public bool Between(RMagnitude min, RMagnitude max) {
      return min.Smaller(this) && this.Smaller(max);
    }
    public bool Greater(RMagnitude other) {
      return !Smaller(other) && this.As<REquality>().Differs(other.As<REquality>());
    }
  }
  public class Circle2 : Does<REquality>, Does<RMagnitude> {
    public int Center { get; set; }
    public int Radius { get; set; }
    public double Area { get { return Math.PI * (Radius * Radius); } }
    public bool Equals(REquality other) {
      var otherCircle = other.As<Circle>();
      if (otherCircle == null) return false;
      return otherCircle.Center == Center && otherCircle.Radius == Radius;
    }
    public bool Smaller(RMagnitude other) {
      var otherCircle = other.As<Circle>();
      if (otherCircle == null) return false;
      return Radius < otherCircle.Radius;
    }
  }

  public static class Ex {
    public static T As<T>(this Role self) where T : class {
      return self as T;
    }
  }

}
