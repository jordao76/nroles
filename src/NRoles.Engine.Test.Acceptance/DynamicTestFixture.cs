using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test {

  public abstract class DynamicTestFixture : MarshalByRefObject {
    public abstract void Test();
  }

}
