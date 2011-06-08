using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  [RoleTest(CompositionType = typeof(Identity_Composition))]
  [RoleTest(
    CompositionType = typeof(Identity_Composition_With_Wrong_Type_Argument))]
    //ExpectedCompositionError = TODO)]
  public class Identity<S> : Role { // S is the type of "this"
    public S Self {
      get { return this.Cast<S>(); }
    }
  }

  public class Identity_Composition : Does<Identity<Identity_Composition>> {
  }

  public class Identity_Composition_With_Wrong_Type_Argument : Does<Identity<Identity_Composition>> {
  }

}
