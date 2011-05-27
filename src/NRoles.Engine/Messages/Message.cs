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
    protected Message(MessageType type, int number, string text, string prefix = "NR") {
      Type = type;
      Number = number;
      Text = text;
      Prefix = prefix;
    }

    /// <summary>
    /// The message type.
    /// </summary>
    public MessageType Type { get; private set; }
    
    /// <summary>
    ///  The message number.
    /// </summary>
    public int Number { get; private set; }
    
    /// <summary>
    /// The message text.
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// The message prefix. Used to disambiguate subsystems.
    /// </summary>
    public string Prefix { get; private set; }

    /// <summary>
    /// Returns a string representation of this message.
    /// </summary>
    /// <returns>Message as a string.</returns>
    public override string ToString() {
      // TODO: use "filename(line,column)" in place of [NRoles] when this information is available
      // this format is understood by Visual Studio
      return string.Format("[NRoles] : {0} {1}{2:0000} : {3}", Type, Prefix, Number, Text);
    }

  }

}
