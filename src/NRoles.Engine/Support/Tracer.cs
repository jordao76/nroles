using System;
using System.Diagnostics;
using Mono.Cecil;

namespace NRoles.Engine {
  class Tracer {
    private static TraceSwitch MutatorSwitch {
      get { return new TraceSwitch("MutatorSwitch", "Trace switch for mutators.", "verbose"); }
    }
    [Conditional("TRACE")]
    public static void TraceVerbose(string message, params object[] parameters) {
      Trace.WriteLineIf(MutatorSwitch.Level == TraceLevel.Verbose, string.Format(message, parameters));
    }
    [Conditional("TRACE")]
    public static void Assert(bool condition = false, string message = null) {
      if (!condition) throw new InvalidProgramException(message ?? "Assert condition failed!");
    }

    // TODO: remove these methods?
    [Conditional("TRACE")]
    public static void DumpType(TypeReference type) {
      TraceVerbose("TYPE DUMP: {0}: {1}", type, type.GetType());
      DumpGenericParameters(type);
      DumpGenericArguments(type);
    }

    private static void DumpGenericArguments(TypeReference type) {
      TraceVerbose("\tGENERIC PARAMETERS: {0}", type.HasGenericParameters);
      foreach (GenericParameter parameter in type.GenericParameters) {
        TraceVerbose("\t\tPARAM: {0}: {1}", parameter, parameter.GetType());
      }
    }

    private static void DumpGenericParameters(TypeReference type) {
      GenericInstanceType genericInstance = type as GenericInstanceType;
      TraceVerbose("\tGENERIC ARGUMENTS: {0}", genericInstance != null && genericInstance.HasGenericArguments);
      if (genericInstance != null) {
        foreach (TypeReference argument in genericInstance.GenericArguments) {
          TraceVerbose("\t\tARG: {0}: {1}", argument, argument.GetType());
        }
      }
    }
  }
}
