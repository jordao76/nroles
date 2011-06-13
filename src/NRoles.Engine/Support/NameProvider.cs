using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine {
  
  public static class NameProvider {

    public static string GetCodeClassName(string roleName) {
      return "Code";
    }
    public static string GetCodeClassInitMethodName(string roleName) {
      return "Init";
    }
    public static string GetInstanceParameterName() {
      return "this";
    }

    public static string GetStateClassName(string roleName) {
      return "Cheshire";
    }
    public static string GetStateClassPropertyName(string roleName) { // TODO: also pass the composition class name?
      return "Smile";
    }
    public static string GetStateClassPropertyGetterName(string roleName) { // TODO: also pass the composition class name?
      return "get_" + GetStateClassPropertyName(roleName);
    }
    // NOTE: there's no setter for the state class

    public static string GetStateClassBackingFieldName(string implementedPropertyName) {
      return "__" + implementedPropertyName + "__backing";
    }

    public static string GetVirtualBaseMethodName(string typeName, string methodName) {
      return "__base__" + methodName;
    }

    public static bool IsVirtualBaseMethod(string methodName) {
      return methodName.StartsWith("__base__") && methodName.Length > "__base__".Length;
    }

    public static string GetOriginalBaseMethodName(string virtualBaseMethodName) {
      if (IsVirtualBaseMethod(virtualBaseMethodName)) {
        return virtualBaseMethodName.Substring("__base__".Length);
      }
      return null;
    }

    public static string GetSelfTypeParameterName() {
      return "S";
    }
  }

}
