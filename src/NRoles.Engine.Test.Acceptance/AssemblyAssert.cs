using System;
using System.IO;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {
  
  public static class AssemblyAssert {

    static string ResolvePEVerifyPath() {
      // .NET 4.0
      
      var peVerifyPath =
        Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
          @"Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools\x64\PEVerify.exe"); // Windows 7
      if (File.Exists(peVerifyPath)) return peVerifyPath;

      peVerifyPath =
        Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
          @"Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\PEVerify.exe"); // Windows XP
      if (File.Exists(peVerifyPath)) return peVerifyPath;
      
      return null;
    }

    public static void Verify(string assemblyPath) {
      var result = new AssemblyVerifier(assemblyPath, ResolvePEVerifyPath()).Verify();
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
