using System;
using System.IO;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {

  public static class AssemblyAssert {

    static string ResolvePEVerifyPath() {

      // .NET 4.6

      var peVerifyPath =
        Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
          @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\PEVerify.exe"); // Windows 10
      if (File.Exists(peVerifyPath)) return peVerifyPath;

      // .NET 4.0

      peVerifyPath =
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
      if (ResolvePEVerifyPath() == null) {
        Console.WriteLine("No PEVerify found, skipping IL verification...");
        return;
      }
      var result = new AssemblyVerifier(assemblyPath, ResolvePEVerifyPath()).Verify();
      result.Messages.ForEach(Console.WriteLine);
      if (!result.Success) {
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
