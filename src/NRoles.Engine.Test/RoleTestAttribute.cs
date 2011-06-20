using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test {

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
  public class RoleTestAttribute : MutationTestAttribute {
    public override Type AnnotatedType {
      get { return RoleType; }
      set { RoleType = value; } 
    }
    public override Type SupportingType {
      get { return CompositionType; }
      set { CompositionType = value; }
    }
  }

}
