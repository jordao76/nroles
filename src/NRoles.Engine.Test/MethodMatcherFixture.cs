using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {

  [TestFixture]
  public class MethodMatcherFixture : AssemblyReadonlyFixture {

    private new MethodDefinition GetMethodByName(Type t, string methodName) {
      var classMethod = base.GetMethodByName(t, methodName);
      return (MethodDefinition)classMethod.ResolveContextualDefinition();
    }

    class Generic<T> { public void Method(T p) { } }
    class Inherited : Generic<int> { }
    class NonGeneric { public void Method(int p) { } }

    [Test]
    public void Test_Generic_Method_With_Type_Argument_Should_Match_Equivalent_Non_Generic_Method() {
      var method1 = GetMethodByName(typeof(Inherited), "Method");
      var method2 = GetMethodByName(typeof(NonGeneric), "Method");
      Assert.IsTrue(MethodMatcher.IsMatch(method1, method2));
    }

    [Test]
    public void Test_Generic_Method_Should_Not_Match_Generic_Method_With_Type_Argument() {
      var method1 = GetMethodByName(typeof(Generic<>), "Method");
      var method2 = GetMethodByName(typeof(Inherited), "Method");
      Assert.IsFalse(MethodMatcher.IsMatch(method1, method2));
    }

    class NonGenericWithString : Generic<string> { }
    [Test]
    public void Test_Generic_Method_With_Type_Argument_Should_Not_Match_Generic_Method_With_Different_Type_Argument() {
      var method1 = GetMethodByName(typeof(NonGenericWithString), "Method");
      var method2 = GetMethodByName(typeof(NonGeneric), "Method");
      Assert.IsFalse(MethodMatcher.IsMatch(method1, method2));
    }

  }

}
