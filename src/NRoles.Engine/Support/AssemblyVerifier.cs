using System;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Checks an assembly for errors. Uses the PEVerify executable.
  /// </summary>
  public class AssemblyVerifier {

    static string[] peVerifySearchPaths = new string[] { 
      @"Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\",
      @"Microsoft SDKs\Windows\v7.0A\bin\",
      @"Microsoft SDKs\Windows\v7.0\Bin\",
      @"Microsoft SDKs\Windows\v6.0A\bin\"
    };

    static string ResolvePEVerifyPath() {
      foreach (var searchPath in peVerifySearchPaths) {
        var peVerifyPath =
          Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            searchPath + "PEVerify.exe");
        if (File.Exists(peVerifyPath)) return peVerifyPath;
      }
      return null;
    }

    readonly AssemblyDefinition _assembly;
    readonly string _assemblyPath;
    int _timeoutInMillis;

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="assembly">Assembly to verify.</param>
    /// <param name="timeoutInMillis">Timeout for the verification process in milliseconds. Valid values are from 5000 to 60000.</param>
    public AssemblyVerifier(AssemblyDefinition assembly, int timeoutInMillis = 5000) {
      if (assembly == null) throw new ArgumentNullException("assembly");
      if (timeoutInMillis < 1000 || timeoutInMillis > 60000) throw new ArgumentOutOfRangeException("timeoutInMillis", "timeout must be between 1 annd 60s (1000 and 60000ms)");
      _assembly = assembly;
      _timeoutInMillis = timeoutInMillis;
      _assemblyPath = null;
    }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="assemblyPath">Path to the assembly to verify.</param>
    /// <param name="timeoutInMillis">Timeout for the verification process in milliseconds. Valid values are from 5000 to 60000.</param>
    public AssemblyVerifier(string assemblyPath, int timeoutInMillis = 5000) {
      if (assemblyPath == null) throw new ArgumentNullException("assemblyPath");
      if (timeoutInMillis < 1000 || timeoutInMillis > 60000) throw new ArgumentOutOfRangeException("timeoutInMillis", "timeout must be between 1 annd 60s (1000 and 60000ms)");
      _assemblyPath = assemblyPath;
      _timeoutInMillis = timeoutInMillis;
      _assembly = null;
    }

    /// <summary>
    /// Checks the assembly for errors and return the result of the operation.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    public IOperationResult Verify() {
      var result = new OperationResult();
      var peVerifyPath = ResolvePEVerifyPath();
      if (peVerifyPath == null) {
        result.AddMessage(Error.PEVerifyDoesntExist());
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
      var peVerify = Process.Start(new ProcessStartInfo {
        FileName = peVerifyPath,
        Arguments = "/nologo \"" + assemblyPath + "\"",
        WorkingDirectory = Path.GetDirectoryName(assemblyPath),
        UseShellExecute = false,
        RedirectStandardOutput = true,
        CreateNoWindow = true
      });
      if (!peVerify.WaitForExit(_timeoutInMillis)) {
        peVerify.Kill();
        result.AddMessage(Error.PEVerifyTimeout(_timeoutInMillis));
      }
      if (peVerify.ExitCode != 0) {
        result.AddMessage(Error.PEVerifyError(peVerify.StandardOutput.ReadToEnd()));
      }
    }

  }

}
