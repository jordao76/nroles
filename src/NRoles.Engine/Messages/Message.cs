using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  /// <summary>
  /// A message that can be returned from the role engine.
  /// </summary>
  public abstract class Message {

    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="number">The message number.</param>
    /// <param name="text">The message text.</param>
    /// <param name="prefix">The message prefix. Used to disambiguate subsystems. The default is "NR", for the NRoles engine.</param>
    /// <param name="sequencePoint">Sequence point information (file, line and column).</param>
    protected Message(MessageType type, int number, string text, string prefix = "NR", SequencePoint sequencePoint = null) {
      Type = type;
      Number = number;
      Text = text;
      Prefix = prefix;
      SequencePoint = sequencePoint;
    }

    /// <summary>
    /// The message type.
    /// </summary>
    public MessageType Type { get; }

    /// <summary>
    ///  The message number.
    /// </summary>
    public int Number { get; }

    /// <summary>
    /// The message text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The message prefix. Used to disambiguate subsystems.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Sequence point information (file, line and column). Can be <c>null</c>.
    /// </summary>
    public SequencePoint SequencePoint { get; }

    /// <summary>
    /// The path to the file where the message occurs. Can be <c>null</c>.
    /// </summary>
    public string File {
      get {
        return SequencePoint?.Document.Url;
      }
    }

    /// <summary>
    /// Line number in the file where the message occurs. Zero if not available.
    /// </summary>
    public int LineNumber {
      get {
        return SequencePoint?.StartLine ?? 0;
      }
    }

    /// <summary>
    /// Column number in the file where the message occurs. Zero if not available.
    /// </summary>
    public int ColumnNumber {
      get {
        return SequencePoint?.StartColumn ?? 0;
      }
    }

    /// <summary>
    /// End line number in the file where the message occurs. Zero if not available.
    /// </summary>
    public int EndLineNumber {
      get {
        return SequencePoint?.EndLine ?? 0;
      }
    }

    /// <summary>
    /// End column number in the file where the message occurs. Zero if not available.
    /// </summary>
    public int EndColumnNumber {
      get {
        return SequencePoint?.EndLine ?? 0;
      }
    }

    /// <summary>
    /// The message code, which is a formatting of the message prefix and number.
    /// </summary>
    public string MessageCode {
      get {
        return $"{Prefix}{Number:0000}";
      }
    }

    /// <summary>
    /// The message type as a string for output. See <see cref="Type"/>.
    /// </summary>
    public string TypeString {
      get {
        return Type.ToString().ToLower();
      }
    }

    /// <summary>
    /// Returns a string representation of this message.
    /// </summary>
    /// <returns>Message as a string.</returns>
    public override string ToString() {
      if (File != null) {
        return $"{File}({LineNumber},{ColumnNumber},{EndLineNumber},{EndColumnNumber}): {TypeString} {MessageCode}: {Text}";
      }
      return $"NRoles: {TypeString} {MessageCode}: {Text}";
    }

  }

}
