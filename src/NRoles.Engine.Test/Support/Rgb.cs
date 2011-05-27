using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {
  
  // TODO: check with a struct. 
  //   It fails because structs have no constructor to add the Init call.
  public class Rgb : Does<REquatable<Rgb>> {
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
    public bool Equals(Rgb other) {
      return R == other.R && G == other.G && B == other.B;
    }
  }

}
