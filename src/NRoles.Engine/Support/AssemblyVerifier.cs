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
    string _peVerifyPath;

    private AssemblyVerifier(string peVerifyPath, int timeoutInMillis) {
      if (peVerifyPath == null) throw new ArgumentNullException("peVerifyPath");
      if (timeoutInMillis < 1000 || timeoutInMillis > 60000) throw new ArgumentOutOfRangeException("timeoutInMillis", "timeout must be between 1 and 60s (1000 and 60000ms)");
      _peVerifyPath = peVerifyPath;
      _timeoutInMillis = timeoutInMillis;
    }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="assembly">Assembly to verify.</param>
    /// <param name="peVerifyPath">Path to the PEVerify executable.</param>
    /// <param name="timeoutInMillis">Timeout for the verification process in milliseconds. Valid values are from 1000 to 60000.</param>
    public AssemblyVerifier(AssemblyDefinition assembly, string peVerifyPath, int timeoutInMillis = 5000) : this(peVerifyPath, timeoutInMillis) {
      if (assembly == null) throw new ArgumentNullException("assembly");
      _assemblyPath = null;
      _assembly = assembly;
    }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="assemblyPath">Path to the assembly to verify.</param>
    /// <param name="peVerifyPath">Path to the PEVerify executable.</param>
    /// <param name="timeoutInMillis">Timeout for the verification process in milliseconds. Valid values are from 1000 to 60000.</param>
    public AssemblyVerifier(string assemblyPath, string peVerifyPath, int timeoutInMillis = 5000) : this(peVerifyPath, timeoutInMillis) {
      if (assemblyPath == null) throw new ArgumentNullException("assemblyPath");
      _assemblyPath = assemblyPath;
      _assembly = null;
    }

    /// <summary>
    /// Checks the assembly for errors and return the result of the operation.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    public IOperationResult Verify() {
      var result = new OperationResult();
      var peVerifyPath = _peVerifyPath;
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

  }

}
