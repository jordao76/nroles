using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {

  [TestFixture]
  public class MethodMatcherFixture : AssemblyReadonlyFixture {

    private MethodDefinition GetMethod(Type t, string methodName) {
      var type = GetType(t);
      var currentType = type;
      TypeReference typeContext = type;
      MethodDefinition method = null;
      while (currentType != null && 
        (method = currentType.Methods.Cast<MethodDefinition>().SingleOrDefault(m => m.Name == methodName)) == null) {
        typeContext = currentType.BaseType;
        currentType = typeContext == null ? null : typeContext.Resolve();
      }
      Assert.IsNotNull(method);
      // the important thing here is to use the member resolver
      return new MemberResolver(typeContext).ResolveMethodDefinition(method);
    }

    class Generic<T> { public void Method(T p) { } }
    class Inherited : Generic<int> { }
    class NonGeneric { public void Method(int p) { } }

    [Test]
    public void Test_Ìnherited_From_Generic_Method_With_Type_Argument_Should_Match_Equivalent_Non_Generic_Method() {
      var method1 = GetMethod(typeof(Inherited), "Method");
      var method2 = GetMethod(typeof(NonGeneric), "Method");
      Assert.IsTrue(MethodMatcher.IsMatch(method1, method2));
    }

    class InheritedGeneric<T> : Generic<T> { }
    [Test]
    public void Test_Generic_Method_Should_Not_Match_Non_Generic_Method() {
      var method1 = GetMethod(typeof(InheritedGeneric<>), "Method");
      var method2 = GetMethod(typeof(NonGeneric), "Method");
      Assert.IsFalse(MethodMatcher.IsMatch(method1, method2));
    }

    class NonGenericWithString : Generic<string> { }
    [Test]
    public void Test_Generic_Method_With_Type_Argument_Should_Not_Match_Different_Non_Generic_Method() {
      var method1 = GetMethod(typeof(NonGenericWithString), "Method");
      var method2 = GetMethod(typeof(NonGeneric), "Method");
      Assert.IsFalse(MethodMatcher.IsMatch(method1, method2));
    }

  }

}
