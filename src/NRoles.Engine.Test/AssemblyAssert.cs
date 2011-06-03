using System;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {
  
  public static class AssemblyAssert {

    public static void Verify(string assemblyPath) {
      var result = new AssemblyVerifier(assemblyPath).Verify();
      result.Messages.ForEach(Console.WriteLine);
      Assert.That(result.Success);
    }

  }

}
