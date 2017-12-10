using System;
using System.Collections.Generic;
using System.Linq;

namespace NRoles.Engine {

  /// <summary>
  /// A container of <see cref="Message"/>s.
  /// </summary>
  /// <seealso cref="Message"/>
  public interface IMessageContainer {

    /// <summary>
    /// Messages in the container.
    /// </summary>
    IEnumerable<Message> Messages { get; }
    
    /// <summary>
    /// Add a message to the container.
    /// </summary>
    /// <param name="message">Message to add.</param>
    void AddMessage(Message message);

  }

  /// <summary>
  /// Canonical <see cref="IMessageContainer"/> implementation.
  /// </summary>
  public class MessageContainer : IMessageContainer {
    private List<Message> _messages = new List<Message>();
    public virtual IEnumerable<Message> Messages {
      get { return _messages; }
    }
    public virtual void AddMessage(Message message) {
      if (message == null) throw new ArgumentNullException("message");
      _messages.Add(message);
    }
  }

  /// <summary>
  /// Extension methods for <see cref="IMessageContainer"/>.
  /// </summary>
  public static class MessageContainerExtensions {

    /// <summary>
    /// Indicates if the message container has errors, i.e., if it contains error messages.
    /// </summary>
    /// <param name="self">Extended instance.</param>
    /// <returns>Whether the extended instance has error messages.</returns>
    public static bool HasError(this IMessageContainer self) {
      if (self == null) throw new InstanceArgumentNullException();
      return self.Messages.Any(message => message.Type == MessageType.Error);
    }

    /// <summary>
    /// Adds all the messages of a message container into another one.
    /// </summary>
    /// <param name="self">Extended instance. Will receive all messages from <paramref name="source"/>.</param>
    /// <param name="source">The message container to draw messages from.</param>
    public static void Slurp(this IMessageContainer self, IMessageContainer source) {
      if (self == null) throw new InstanceArgumentNullException();
      if (source == null) return;
      source.Messages.ForEach(message => self.AddMessage(message));
    }

  }

}
