using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  class Pair<TFirst, TSecond> {
    public readonly TFirst First;
    public readonly TSecond Second;
    public Pair(TFirst first, TSecond second) {
      First = first;
      Second = second;
    }
  }

}
