using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using NDesk.Options;
using NRoles.Engine;

namespace NRoles.App {

  class Program {

    static int Main(string[] args) {

      var timer = new Stopwatch();
      timer.Start();

      bool trace = false;
      bool quiet = false;
      bool warningsAsErrors = false;
      string input = null;
      string output = null;
      bool verify = false;
      int verifyTimeout = 5; // default 5s
      bool showHelp = false;

      var options = new OptionSet { 
        { "h|help", "show options.", h => showHelp = h != null },
        { "q|quiet", "shhh.", q => quiet = q != null },
        { "evilwarnings", "treats warnings as errors.", wae => warningsAsErrors = wae != null },
        { "o|out=", "output to VALUE (default is to overwrite the input file).", o => output = o },
        { "v|verify", "verifies the resulting assembly with PEVerify.exe", v => verify = v != null },
        { "verifytimeout=", "sets the timeout in seconds to wait for PEVerify to check the generated assembly. Only makes sense with the --verify option set.", (int vt) => verifyTimeout = vt },
        { "t|trace", "prints trace information.", t => trace = t != null }
      };

      List<string> unnamed;
      try {
        unnamed = options.Parse(args);
      }
      catch (OptionException e) {
        ShowError(e.Message);
        return -1;
      }

      if (!quiet) {
        Console.WriteLine("NRoles v" + _Metadata.Version);
      }

      if (showHelp) {
        ShowHelp(options);
        return 0;
      }

      if (trace) {
        SetUpTraceListener();
      }

      if (unnamed.Count != 1) {
        var invalid = string.Join(", ", unnamed.ToArray());
        if (invalid.Length > 0) invalid = " Invalid parameters: " + invalid;
        ShowError("Provide valid parameters and one input assembly." + invalid);
        return -1;
      }
      input = unnamed[0];

      IOperationResult result;
      try {
        result = new RoleEngine().Execute(
          new RoleEngineParameters(input, output) { 
            TreatWarningsAsErrors = warningsAsErrors,
            RunPEVerify = verify,
            PEVerifyTimeout = verifyTimeout
          });
      }
      catch (Exception ex) {
        result = new OperationResult();
        result.AddMessage(Error.InternalError());
        if (trace) {
          Console.WriteLine("Failed!");
          Console.WriteLine();
          Console.WriteLine(ex.ToString());
          Console.WriteLine();
          Console.WriteLine("HRESULT 0x{0:x}", Marshal.GetHRForException(ex));
        }
      }

      timer.Stop();
      result.Messages.ForEach(message => Console.WriteLine(message));
      if (!quiet) {
        // TODO: print statistics? timing, number of roles, number of compositions, etc... <= these would be like info messages...
        Console.WriteLine("Done, took {0}s", (timer.ElapsedMilliseconds / 1000f)); 
      }
      if (!result.Success) return -1;
      return 0;
    }

    private static void ShowError(string message) {
      Console.Write("nutate: ");
      Console.WriteLine(message);
      Console.WriteLine("Try 'nutate --help' for available options.");
    }

    private static void ShowHelp(OptionSet options) {
      Console.WriteLine("Usage: nutate [options] <input>");
      Console.WriteLine("Mutates an input assembly to enable roles. For more info: https://github.com/jordao76/nroles");
      Console.WriteLine();
      Console.WriteLine("Options:");
      options.WriteOptionDescriptions(Console.Out);
    }

    private static void SetUpTraceListener() {
      var consoleListener = new TextWriterTraceListener(Console.Out);
      Trace.Listeners.Add(consoleListener);
    }

  }

}
