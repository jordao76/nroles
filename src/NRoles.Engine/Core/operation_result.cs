using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  /// <summary>
  /// Encapulates the result of an operation.
  /// </summary>
  /// <remarks>
  /// The result of an operation includes whether it was successful and 
  /// any generated messages.
  /// </remarks>
  public interface IOperationResult : IMessageContainer {

    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    bool Success { get; }
  }

  /// <summary>
  /// Encapulates the result of an operation. Default implementation for <see cref="IOperationResult"/>. 
  /// </summary>
  public class OperationResult : IOperationResult {
    List<Message> _messages = new List<Message>();

    /// <summary>
    /// The messages of the operation result.
    /// </summary>
    public IEnumerable<Message> Messages { get { return _messages; } }

    /// <summary>
    /// Adds a message to this operation result.
    /// </summary>
    /// <param name="message">Message to add.</param>
    public void AddMessage(Message message) {
      if (message == null) throw new ArgumentNullException("message");
      _messages.Add(message); 
    }

    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    /// <remarks>
    /// Just checks if there are any error messages in this operation result's messages.
    /// </remarks>
    public bool Success { 
      get { return !this.HasError(); } 
    }
  }

  /// <summary>
  /// An operation result that encapsulates other operation results.
  /// </summary>
  public class CompositeOperationResult : IOperationResult {
    List<IOperationResult> _children = new List<IOperationResult>();
    OperationResult _defaultResult = new OperationResult();

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    public CompositeOperationResult() {
      _children.Add(_defaultResult);
    }

    /// <summary>
    /// The messages of the operation result. Combines all messages of the encapsulated results.
    /// </summary>
    public IEnumerable<Message> Messages {
      get {
        return _children.SelectMany(child => child.Messages);
      }
    }

    /// <summary>
    /// Adds a message to this operation result.
    /// </summary>
    /// <param name="message">Message to add.</param>
    public void AddMessage(Message message) {
      if (message == null) throw new ArgumentNullException("message");
      _defaultResult.AddMessage(message);
    }

    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    /// <remarks>
    /// Checks if all of the encapsulated results are successful.
    /// </remarks>
    public bool Success {
      get { return _children.All(child => child.Success); }
    }

    /// <summary>
    /// Adds an operation result to this instance's encapsulated results.
    /// </summary>
    /// <param name="result">Result to add.</param>
    public virtual void AddResult(IOperationResult result) {
      if (result == null) return;
      if (_children.Contains(result)) return; // this doesn't detect cycles if a child is also a composite
      _children.Add(result);
    }

  }

}
