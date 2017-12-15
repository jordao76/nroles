using System;
using System.IO;

namespace NRoles.Engine {

  /// <summary>
  /// Used to create a temporary, disposable file.
  /// Creates the file on the constructor, deletes it on dispose (<see cref="Dispose"/>).
  /// </summary>
  public sealed class TemporaryFile : IDisposable {

    /// <summary>
    /// Creates a new instance of this class. 
    /// Creates a temporary file with a random name on the supplied directory or the 
    /// system's temporary folder.
    /// The file name can be accessed through the <see cref="FilePath"/> property.
    /// </summary>
    /// <param name="directory">Directory to use.</param>
    public TemporaryFile(string directory = null) {
      Create(Path.Combine(directory ?? Path.GetTempPath(), Path.GetRandomFileName()));
    }

    /// <summary>
    /// Disposes of this instance. Deletes the created temporary file.
    /// </summary>
    public void Dispose() {
      Delete();
    }

    /// <summary>
    /// The full file path (directory + name) of the generated temporary file.
    /// Will be null if the file has already been deleted (<see cref="Dispose"/>).
    /// </summary>
    public string FilePath { get; private set; }

    private void Create(string path) {
      FilePath = path;
      using (File.Create(FilePath)) { };
    }

    private void Delete() {
      File.Delete(FilePath);
      FilePath = null;
    }
  }

}
