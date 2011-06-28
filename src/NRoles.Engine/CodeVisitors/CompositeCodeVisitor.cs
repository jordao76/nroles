using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  class CompositeCodeVisitor : CompositeCodeVisitorBase {
    List<CodeVisitorBase> _visitors = new List<CodeVisitorBase>();
    
    public override void AddCodeVisitor(CodeVisitorBase visitor) {
      if (visitor == null) throw new ArgumentNullException("visitor");
      _visitors.Add(visitor);
    }
    
    internal override void Apply(Action<CodeVisitorBase> action) {
      _visitors.ForEach(action);
    }

    public int Count { get { return _visitors.Count; } }
  }

}
