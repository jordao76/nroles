using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test {
  
  public static class OperationAssert {
  
    public static void IsSuccessful(IOperationResult result) {
      if (!result.Success) {
        var messages = result.Messages.ToArray();
        messages.ForEach(Console.WriteLine);
        Assert.Fail("Operation should succeed but failed with {0} message(s)",
          messages.Length);
      }
    }

    public static void Failed(IOperationResult result) {
      if (result.Success) {
        Assert.Fail("Operation should fail but succeeded");
      }
    }

  }

}
