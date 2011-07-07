using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  [RoleTest(
    OtherRoles = new Type[] { typeof(REquatable<>) },
    CompositionType = typeof(RColor))]
  [RoleTest(
    OtherRoles = new Type[] { typeof(REquatable<>), typeof(RComparable<>), typeof(RCircle) },
    CompositionType = typeof(ColoredCircle),
    OtherCompositions = new Type[] { typeof(RComparable<>), typeof(RCircle), typeof(RColor) })]
  public class RColor : Does<REquatable<RColor>>, Role {
    public Rgb Rgb { get; set; }
    public bool Equals(RColor other) {
      return Rgb.Equals(other.Rgb);
    }
  }

}
