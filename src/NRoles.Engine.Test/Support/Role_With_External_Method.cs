using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NRoles.Engine.Test.Support {

  [RoleTest(ExpectedRoleError = Error.Code.RoleHasPInvokeMethod)]
  public class Role_With_External_Method : Role {
    [DllImport("whatever")]
    public static extern void Method();
  }

}
