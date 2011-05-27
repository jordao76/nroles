using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  [RoleTest(
    OtherRoles = new Type[] { typeof(RComparable<>), typeof(RCircle) },
    CompositionType = typeof(RCircle),
    OtherCompositions = new Type[] { typeof(RComparable<>) })]
  [RoleTest(CompositionType = typeof(Rgb))]
  public abstract class REquatable<T> : IEquatable<T>, Role {
    public abstract bool Equals(T other);
    public bool Differs(T other) {
      return !Equals(other);
    }
  }
  public abstract class RComparable<T> : IComparable<T>, Does<REquatable<T>>, Role {
    public bool Equals(T other) {
      return CompareTo(other) == 0;
    }
    public abstract int CompareTo(T other);
    public bool LessThan(T other) {
      return CompareTo(other) < 0;
    }
    public bool GreaterThan(T other) {
      return CompareTo(other) > 0;
    }
    public bool LessThanOrEqualTo(T other) {
      return CompareTo(other) <= 0;
    }
    public bool GreaterThanOrEqualTo(T other) {
      return CompareTo(other) >= 0;
    }
    public bool IsBetween(T min, T max) {
      return GreaterThan(min) && LessThan(max);
    }
  }
  public class RCircle : Does<REquatable<RCircle>>, Does<RComparable<RCircle>>, Role {
    public Point Center { get; set; }
    public int Radius { get; set; }
    public double Area { get { return Math.PI * (Radius * Radius); } }
    public int CompareTo(RCircle other) {
      if (other == null) return 1; // non-null > null
      return Radius.CompareTo(other.Radius);
    }
  }

  public struct Point {
    public int X { get; set; }
    public int Y { get; set; }
  }

}
