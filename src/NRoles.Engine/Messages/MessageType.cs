using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {

  /// <summary>
  /// The kinds of messages that can be generated from the roles engine.
  /// </summary>
  public enum MessageType {

    /// <summary>
    /// Error message.
    /// </summary>
    Error,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Information message.
    /// </summary>
    // TODO: what's the right name to appear in the Messages tab in Visual Studio? "Hint"?
    Info

  }

}
