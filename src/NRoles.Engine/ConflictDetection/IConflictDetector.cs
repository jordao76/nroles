namespace NRoles.Engine {

  /// <summary>
  /// A conflict detector.
  /// </summary>
  public interface IConflictDetector {

    /// <summary>
    /// Do the conflict detection.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    ConflictDetectionResult Process();

  }

}
