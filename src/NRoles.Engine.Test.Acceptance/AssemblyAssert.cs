using System;
using System.IO;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {
  
  public static class AssemblyAssert {

    public static void Verify(string assemblyPath) {
      var result = new AssemblyVerifier(assemblyPath).Verify();
      result.Messages.ForEach(Console.WriteLine);
      if (!result.Success) {
        // TODO: ildasm dump
        //   ildasm /text /tokens /caverbal <assemblyPath>
        if (Directory.Exists(@"C:\Temp")) {
          File.Copy(assemblyPath, @"C:\Temp\NRoles.mutated.dll", true);
          Console.WriteLine(@"SAVED ASSEMBLY: C:\Temp\NRoles.mutated.dll");
        }
      }
      OperationAssert.IsSuccessful(result);
    }

  }

}
