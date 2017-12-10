using System;
using System.IO;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {

  public static class AssemblyAssert {

    public static void Verify(string assemblyPath) {
      var result = new AssemblyVerifier(assemblyPath).Verify();
      if (!result.Success) {
        result.Messages.ForEach(Console.WriteLine);
        // TODO: ildasm dump
        //   ildasm /text /tokens /caverbal <assemblyPath>
        var savePath = Path.ChangeExtension(assemblyPath, "mutated.dll");
        File.Copy(assemblyPath, savePath, true);
        Console.WriteLine($"FAILED ASSEMBLY: {savePath}");
        Assert.Fail("Failed PEVerify");
      }
    }

  }

}
