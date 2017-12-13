using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NRoles.Engine;

namespace NRoles.Build {

  public class RoleTask : Task {

    [Required]
    public string AssemblyPath { get; set; }
    [Required]
    public string References { get; set; } 
    public bool ShowTrace { get; set; }
    public bool TreatWarningsAsErrors { get; set; }

    public override bool Execute() {
      var timer = new Stopwatch();
      timer.Start();
      Log.LogMessage("NRoles v" + _Metadata.Version);

      if (ShowTrace) {
        SetUpTraceListener();
      }

      IOperationResult result;
      try {
        result = new RoleEngine().Execute(
          new RoleEngineParameters(AssemblyPath) { 
            TreatWarningsAsErrors = TreatWarningsAsErrors,
            RunPEVerify = false,
            References = References.Split(';')
          });
        LogMessages(result);
      }
      catch (Exception ex) {
        // TODO: Error.InternalError() should be processed inside the engine! Also look in NRoles.App
        result = new OperationResult();
        result.AddMessage(Error.InternalError());
        LogMessages(result);
        Log.LogErrorFromException(ex);
      }

      timer.Stop();
      Log.LogMessage("NRoles done, took {0}s", (timer.ElapsedMilliseconds / 1000f)); 
      return result.Success;
    }

    private void LogMessages(IMessageContainer messageContainer) {
      foreach (var message in messageContainer.Messages) {
        switch (message.Type) {
          case MessageType.Error:
            Log.LogError(subcategory: null, errorCode: message.MessageCode, helpKeyword: null, file: message.File,
              lineNumber: message.LineNumber, columnNumber: message.ColumnNumber, endLineNumber: message.EndLineNumber, endColumnNumber: message.EndColumnNumber, 
              message: message.Text);
            break;
          case MessageType.Warning:
            Log.LogWarning(subcategory: null, warningCode: message.MessageCode, helpKeyword: null, file: message.File,
              lineNumber: message.LineNumber, columnNumber: message.ColumnNumber, endLineNumber: message.EndLineNumber, endColumnNumber: message.EndColumnNumber,
              message: message.Text);
            break;
          case MessageType.Info: Log.LogMessage(message.ToString()); break;
        }
      }
    }

    private void SetUpTraceListener() {
      var listener = new BuildLogTraceListener(Log);
      Trace.Listeners.Add(listener);
    }

  }
  

}
