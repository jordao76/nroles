using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  [RoleTest]
  public abstract class RComparable<T> : Role {
    public abstract bool Smaller(T other);
    public bool Greater(T other) {
      // this is actually GreaterOrEquals
      return !Smaller(other);
    }
    public bool Between(T min, T max) {
      return this.Greater(min) && this.Smaller(max);
    }
  }

}
