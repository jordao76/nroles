using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NRoles.Engine {
  
  public static class EnumerableExtensions {

    public static void ForEach<T>(this IEnumerable<T> self, Action<T> action) {
      if (self == null) throw new InstanceArgumentNullException();
      if (action == null) throw new ArgumentNullException("action");
      foreach (T element in self) {
        action(element);
      }
    }
     
    public static IEnumerable<T> Assert<T>(this IEnumerable<T> self, Predicate<IEnumerable<T>> conditionPredicate) { // TODO: remove this?
      if (self == null) throw new InstanceArgumentNullException();
      if (conditionPredicate == null) throw new ArgumentNullException("assert");
      Tracer.Assert(conditionPredicate(self));
      return self;
    }

    public static IEnumerable<T> Trace<T>(this IEnumerable<T> self, string title = null) {
      if (self == null) throw new InstanceArgumentNullException();
      if (title != null) Tracer.TraceVerbose(title);
      self.ForEach(item => Tracer.TraceVerbose(item.ToString()));
      if (title != null) Tracer.TraceVerbose("/" + title);
      return self;
    }

  }

}
