using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Build.Utilities;

namespace NRoles.Build {
  
  public class BuildLogTraceListener : TraceListener {

    private readonly TaskLoggingHelper _logger;
    private readonly StringBuilder _pending = new StringBuilder();

    public BuildLogTraceListener(TaskLoggingHelper logger) {
      _logger = logger;
    }

    public override void Write(string message) {
      _pending.Append(message);
    }

    public override void WriteLine(string message) {
      _pending.Append(message);
      WritePendingMessages();
    }

    public override void Flush() {
      WritePendingMessages();
      base.Flush();
    }

    public override void Close() {
      WritePendingMessages();
      base.Close();
    }
    
    private void WritePendingMessages() {
      if (_pending.Length > 0) {
        _logger.LogMessage(_pending.ToString());
        _pending.Clear();
      }      
    }

  }

}
