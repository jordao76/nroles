using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  public interface IEquality {
    bool Equals(IEquality other);
    bool Differs(IEquality other);
  }
  [RoleTest(
    OtherRoles = new Type[] { typeof(Magnitude) },
    CompositionType = typeof(Circle))]
  public abstract class Equality : IEquality, Role {
    public abstract bool Equals(IEquality other);
    public bool Differs(IEquality other) { return !Equals(other); }
  }
  public interface IMagnitude : IEquality {
    bool Smaller(IMagnitude other);
    bool Greater(IMagnitude other);
    bool Between(IMagnitude min, IMagnitude max);
  }
  public abstract class Magnitude : IMagnitude, Role {
    public abstract bool Smaller(IMagnitude other);
    public abstract bool Equals(IEquality other);
    public abstract bool Differs(IEquality other);
    public bool Between(IMagnitude min, IMagnitude max) {
      return min.Smaller(this) && this.Smaller(max);
    }
    public bool Greater(IMagnitude other) {
      return !Smaller(other) && this.Differs(other);
    }
  }
  public class Circle : Does<Equality>, Does<Magnitude> {
    public int Center { get; set; }
    public int Radius { get; set; }
    public double Area { get { return Math.PI * (Radius * Radius); } }
    public bool Equals(IEquality other) {
      var otherCircle = other as Circle;
      if (otherCircle == null) return false;
      return otherCircle.Center == Center && otherCircle.Radius == Radius;
    }
    public bool Smaller(IMagnitude other) {
      var otherCircle = other as Circle;
      if (otherCircle == null) return false;
      return Radius < otherCircle.Radius;
    }
  }

}
