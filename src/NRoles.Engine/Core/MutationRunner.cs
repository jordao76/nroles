using System;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// Orchestrates the execution of a series of mutators on a given assembly or on a given type.
  /// </summary>
  /// <seealso cref="IMutator"/>
  public class MutationRunner {
    AssemblyDefinition _sourceAssembly;
    TypeDefinition _sourceType;

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="sourceAssembly">Assembly that is the target of the mutations.</param>
    public MutationRunner(AssemblyDefinition sourceAssembly) {
      if (sourceAssembly == null) throw new ArgumentNullException("sourceAssembly");
      _sourceAssembly = sourceAssembly;
    }

    /// <summary>
    /// Creates a new instance of this class that will scope the mutations to te given type.
    /// </summary>
    /// <param name="sourceType">Type that is the source of the mutations. Will scope the mutations to just this type.</param>
    public MutationRunner(TypeDefinition sourceType) {
      if (sourceType == null) throw new ArgumentNullException("sourceType");
      _sourceAssembly = sourceType.Module.Assembly;
      _sourceType = sourceType;
    }

    /// <summary>
    /// Executes the given mutators.
    /// </summary>
    /// <param name="mutators">Mutators.</param>
    /// <returns>The composite result of all mutators' mutations.</returns>
    public IOperationResult Run(params IMutator[] mutators) {
      if (mutators == null || mutators.Length == 0) return null;

      var context = new MutationContext(_sourceAssembly.MainModule);
      var parameters = new MutationParameters {
        Context = context,
        SourceType = _sourceType
      };

      var result = Mutate(mutators, parameters);
      context.Finalize(result);

      return result;
    }

    private IOperationResult Mutate(IMutator[] mutators, MutationParameters parameters) {
      var compositeResult = new CompositeOperationResult();
      foreach (var mutator in mutators) {
        var result = mutator.Mutate(parameters);
        compositeResult.AddResult(result);
      }
      return compositeResult;
    }

  }

}
