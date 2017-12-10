using System;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Checks an assembly for errors. Uses the PEVerify executable.
  /// </summary>
  public class AssemblyVerifier {

    readonly AssemblyDefinition _assembly;
    readonly string _assemblyPath;
    int _timeoutInMillis;

    private AssemblyVerifier(int timeoutInMillis) {
      if (timeoutInMillis < 1000 || timeoutInMillis > 60000) throw new ArgumentOutOfRangeException("timeoutInMillis", "timeout must be between 1 and 60s (1000 and 60000ms)");
      _timeoutInMillis = timeoutInMillis;
    }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="assembly">Assembly to verify.</param>
    /// <param name="timeoutInMillis">Timeout for the verification process in milliseconds. Valid values are from 1000 to 60000.</param>
    public AssemblyVerifier(AssemblyDefinition assembly, int timeoutInMillis = 5000) {
      _assembly = assembly ?? throw new ArgumentNullException("assembly");
      _assemblyPath = null;
    }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="assemblyPath">Path to the assembly to verify.</param>
    /// <param name="timeoutInMillis">Timeout for the verification process in milliseconds. Valid values are from 1000 to 60000.</param>
    public AssemblyVerifier(string assemblyPath, int timeoutInMillis = 5000) : this(timeoutInMillis) {
      _assemblyPath = assemblyPath ?? throw new ArgumentNullException("assemblyPath");
      _assembly = null;
    }

    /// <summary>
    /// Checks the assembly for errors and return the result of the operation.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    public IOperationResult Verify() {
      var result = new OperationResult();
      var peVerifyPath = ResolvePEVerifyPath();
      if (!File.Exists(peVerifyPath)) {
        result.AddMessage(Error.PEVerifyDoesntExist(peVerifyPath));
        return result;
      }
      if (_assembly != null) {
        using (var assemblyFile = new TemporaryFile(Directory.GetCurrentDirectory())) {
          _assembly.Write(assemblyFile.FilePath);
          Verify(assemblyFile.FilePath, peVerifyPath, result);
        }
      }
      else {
        Verify(_assemblyPath, peVerifyPath, result);
      }
      return result;
    }

    private void Verify(string assemblyPath, string peVerifyPath, IOperationResult result) {
      Tracer.TraceVerbose("Running PEVerify...");
      using (var peVerify = Process.Start(new ProcessStartInfo {
        FileName = peVerifyPath,
        Arguments = "/nologo \"" + assemblyPath + "\"",
        WorkingDirectory = Path.GetDirectoryName(assemblyPath),
        UseShellExecute = false,
        RedirectStandardOutput = true,
        CreateNoWindow = true
      })) {
        if (!peVerify.WaitForExit(_timeoutInMillis)) {
          peVerify.Kill();
          result.AddMessage(Error.PEVerifyTimeout(_timeoutInMillis));
        }
        if (peVerify.ExitCode != 0) {
          result.AddMessage(Error.PEVerifyError(peVerify.StandardOutput.ReadToEnd()));
        }
      }
    }

    private static string _peverify; // Monostate
    private string ResolvePEVerifyPath() {

      if (_peverify != null) return _peverify;

      var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      var pfx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

      string[] paths = {
        Path.Combine(pf, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools"),
        Path.Combine(pfx86, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools"),
        Path.Combine(pf, @"Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools"),
        Path.Combine(pfx86, @"Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools"),
        Path.Combine(pf, @"Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools"),
        Path.Combine(pfx86, @"Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools"),
        Path.Combine(pf, @"Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools"),
        Path.Combine(pf, @"Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools\x64"),
        Path.Combine(pfx86, @"Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools"),
        Path.Combine(pf, @"Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools"),
        Path.Combine(pfx86, @"Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools"),
        Path.Combine(pf, @"Microsoft SDKs\Windows\v7.0A\Bin"),
        Path.Combine(pfx86, @"Microsoft SDKs\Windows\v7.0A\Bin"),
        Path.Combine(pf, @"Microsoft SDKs\Windows\v6.0A\Bin"),
        Path.Combine(pfx86, @"Microsoft SDKs\Windows\v6.0A\Bin"),
        Path.Combine(pfx86, @"Microsoft Visual Studio 8\SDK\v2.0\bin")
      };

      foreach (var path in paths) {
        var peverify = Path.Combine(path, "PEVerify.exe");
        if (File.Exists(peverify)) {
          return _peverify = peverify;
        }
      }

      return null;
    }

  }

}
