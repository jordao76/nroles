namespace NRoles.Engine {

  /// <summary>
  /// A mutator for an assembly or a type.
  /// </summary>
  public interface IMutator {

    /// <summary>
    /// Mutates an assembly or type given the input parameters and returns the result of the mutation.
    /// </summary>
    /// <param name="parameters">Assembly mutation parameters.</param>
    /// <returns>The result of the mutation.</returns>
    IOperationResult Mutate(MutationParameters parameters);
  
  }

}
