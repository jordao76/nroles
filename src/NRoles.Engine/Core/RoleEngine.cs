using System;
using System.Linq;
using System.IO;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// The entry point for the roles mutations engine.
  /// </summary>
  /// <remarks>
  /// The roles engine will mutate an assembly to enable roles and compositions.
  /// </remarks>
  public class RoleEngine {

    /// <summary>
    /// Executes the roles mutations with the supplied parameters.
    /// </summary>
    /// <param name="parameters">Parameters for the roles mutations.</param>
    /// <returns>The result of the roles mutations, with a success indication and any possible messages.</returns>
    public RoleEngineResult Execute(RoleEngineParameters parameters) {
      parameters.Validate();

      var assembly = ReadAssembly(parameters);

      var result = Mutate(assembly, parameters);

      if (result.Success && result.AssemblyChanged) {
        WriteAssembly(parameters, assembly);
      }

      return result;
    }

    AssemblyDefinition ReadAssembly(RoleEngineParameters parameters) {
      var assemblyPath = parameters.AssemblyPath;
      var assembly = AssemblyDefinition.ReadAssembly(assemblyPath,
        new ReaderParameters() { 
          ReadSymbols = false
          //SymbolReaderProvider = new Mono.Cecil.Pdb.PdbReaderProvider() // Microsoft centric
        }
      );
      return assembly;
    }

    void WriteAssembly(RoleEngineParameters parameters, AssemblyDefinition assembly) {
      assembly.Write(parameters.OutputAssemblyPath,
        new WriterParameters() { 
          WriteSymbols = false,
          //SymbolWriterProvider = new Mono.Cecil.Pdb.PdbWriterProvider() // Microsoft centric
        }
      );
    }

    /// <summary>
    /// Performs the roles mutations on the supplied assembly.
    /// </summary>
    /// <param name="assembly">Assembly to perform the roles mutation on.</param>
    /// <param name="parameters">Parameters for the roles mutations.</param>
    /// <returns>The result of the roles mutation.</returns>
    public RoleEngineResult Mutate(AssemblyDefinition assembly, RoleEngineParameters parameters) {

      var result = new RoleEngineResult();

      // checks if the assembly should not be mutated
      if (!ShouldMutate(assembly)) {
        result.AddMessage(Warning.AssemblyMarkedWithDontMutate(assembly.Name));
        return result;
      }

      var runner = new MutationRunner(assembly);

      // Phase 0: Global checks

      var globalChecksResult = runner.Run(new GlobalRoleChecks());
      result.AddResult(globalChecksResult);
      if (!result.Success) return result;

      // Phase 1: Mutate Roles

      var rolesResult = runner.Run(new MorphIntoRoleMutator());
      result.AddResult(rolesResult);
      if (!result.Success) return result;

      // Phase 2: Compose Roles

      var compositionsResult = runner.Run(new RoleComposerAssemblyMutator());
      result.AddResult(compositionsResult);
      if (!result.Success) return result;

      // treat warnings as errors
      if (parameters.TreatWarningsAsErrors && result.Messages.Any(message => message.Type == MessageType.Warning)) {
        result.AddMessage(Error.ErrorFromWarnings());
        return result;
      }

      // marks the assembly as processed
      MarkAsMutated(assembly);

      if (parameters.RunPEVerify) {
        var assemblyVerificationResult = VerifyAssembly(assembly, parameters.PEVerifyPath, parameters.PEVerifyTimeout);
        result.AddResult(assemblyVerificationResult);
        if (!assemblyVerificationResult.Success) return result;
      }

      result.AssemblyChanged = true;
      return result;
    }

    bool ShouldMutate(AssemblyDefinition assembly) {
      return 
        !assembly.CustomAttributes.Any(ca => 
          ca.Is<DontMutateAttribute>());
    }

    void MarkAsMutated(AssemblyDefinition assembly) {
      assembly.CustomAttributes.Add(
        assembly.MainModule.Create<DontMutateAttribute>());
    }

    IOperationResult VerifyAssembly(AssemblyDefinition assembly, string peVerifyPath, int timeoutInSeconds) {
      return new AssemblyVerifier(assembly, peVerifyPath, timeoutInSeconds * 1000).Verify();
    }

  }

  /// <summary>
  /// The parameters for the roles mutations performed by the roles engine.
  /// </summary>
  /// <seealso cref="RoleEngine"/>
  public class RoleEngineParameters {

    /// <summary>
    /// The path to the assembly to perform the roles mutations on.
    /// </summary>
    public readonly string AssemblyPath;

    /// <summary>
    /// The path to the output assembly for saving the mutations on. 
    /// If it's the same as <see cref="AssemblyPath"/>, the input assembly will be overwritten.
    /// </summary>
    public readonly string OutputAssemblyPath;

    /// <summary>
    /// Indicates if verification of the generated assembly should be executed with the PEVerify tool.
    /// </summary>
    public bool RunPEVerify { get; set; }

    /// <summary>
    /// Path to the PEVerify executable.
    /// </summary>
    public string PEVerifyPath { get; set; }

    /// <summary>
    /// The number of seconds to wait for the PEVerify tool to run.
    /// The default is 5 seconds.
    /// </summary>
    public int PEVerifyTimeout { get; set; }

    /// <summary>
    /// Indicates if warnings will be treated as errors.
    /// </summary>
    public bool TreatWarningsAsErrors { get; set; }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="assemblyPath">The path to the input assembly to perform the roles mutations on.</param>
    /// <param name="outputAssemblyPath">
    /// The path to the output assembly to save the mutated assembly on.
    /// If not supplied, it will be taken from the <paramref name="assemblyPath"/>.
    /// If this file already exists, it will be overwritten.
    /// </param>
    public RoleEngineParameters(string assemblyPath, string outputAssemblyPath = null) { 
      AssemblyPath = assemblyPath;
      OutputAssemblyPath = outputAssemblyPath ?? AssemblyPath;
      PEVerifyTimeout = 5;
    }

    /// <summary>
    /// Validates the parameters to the roles mutation.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the <see cref="AssemblyPath"/> is null.</exception>
    /// <exception cref="InvalidOperationException">If the <see cref="AssemblyPath"/> does not exist in the file system.</exception>
    /// <exception cref="InvalidOperationException">If the <see cref="OutputAssemblyPath"/> directory does not exist in the file system.</exception>
    public void Validate() {
      if (AssemblyPath == null) throw new InvalidOperationException("AssemblyPath is null");
      if (!File.Exists(AssemblyPath)) throw new InvalidOperationException("AssemblyPath is invalid");

      if (Path.IsPathRooted(OutputAssemblyPath)) {
        if (!Directory.Exists(Path.GetDirectoryName(OutputAssemblyPath))) throw new InvalidOperationException("OutputAssemblyPath directory is invalid");
      }
      else { 
        // TODO: get the working directory and concatenate with the OutputAssemblyPath directory to check for directory existence!
      }
      
    }
  }

  /// <summary>
  /// The result of the role engine mutation operations.
  /// </summary>
  /// <seealso cref="RoleEngine"/>
  public class RoleEngineResult : CompositeOperationResult {
    /// <summary>
    /// Indicates if the assembly was changed during the roles mutation operation.
    /// </summary>
    public bool AssemblyChanged { get; set; }
  }

}
