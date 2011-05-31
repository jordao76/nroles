using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NRoles.Engine.Test {
  static class StringExtensions {

    public static string Remove(this string self, string rx) {
      if (self == null) throw new InstanceArgumentNullException();
      if (rx == null) throw new ArgumentNullException();
      return new Regex(rx).Replace(self, "");
    }

    public static string Remove(this string self, string rx, int count) {
      if (self == null) throw new InstanceArgumentNullException();
      if (rx == null) throw new ArgumentNullException();
      return new Regex(rx).Replace(self, "", count);
    }

  }
}
