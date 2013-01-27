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

    public static void AssertWarning(this IOperationResult result, Warning.Code warning) {
      if (!result.Messages.Any(m => m.Number == (int)warning)) {
        Assert.Fail("Expected warning '{0}' not found.", warning);
      }
    }

    public static void AssertError(this IOperationResult result, Error.Code error) {
      if (!result.Messages.Any(m => m.Number == (int)error)) {
        Assert.Fail("Expected error '{0}' not found.", error);
      }
    }
    
  }

}
