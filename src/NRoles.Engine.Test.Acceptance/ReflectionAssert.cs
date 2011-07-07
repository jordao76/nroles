using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test {
  static class ReflectionAssert {
    public static void HasMethod(string expectedMethodName, Type type) {
      var method = type.GetMethod(expectedMethodName);
      Assert.IsNotNull(method);
    }
    public static void HasntMethod(string expectedMethodName, Type type) {
      var method = type.GetMethod(expectedMethodName);
      Assert.IsNull(method);
    }
    public static void HasInterfaceMap(IDictionary<string, string> expectedMap, Type type, Type interfaceType) {
      var map = type.GetInterfaceMap(interfaceType);
      foreach (var entry in expectedMap) {
        int index = map.InterfaceMethods.ToList().FindIndex(method => method.Name == entry.Key);
        Assert.AreNotEqual(-1, index);
        var targetMethod = map.TargetMethods[index].Name;
        Assert.AreEqual(entry.Value, targetMethod);
      }
    }
  }
}
