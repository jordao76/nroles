using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Composition.SelfType {

  [TestFixture]
  public class Composition_Self_Types_Checker_Fixture : AssemblyReadonlyFixture {

    class RSelfType<TSelf> : Role { }

    private IOperationResult Check(Type composition) {
      var checker = new SelfTypeChecker();
      return checker.CheckComposition(GetType(composition));
    }
    
    [Test]
    public void Test_Self_Type_Matches_Host_Type_Should_Succeed() {
      var result = Check(typeof(SelfType));
      Assert.That(result.Success);
      Assert.AreEqual(0, result.Messages.Count());
    }
    class SelfType : Does<RSelfType<SelfType>> { }

    [Test]
    public void Test_Wrong_Self_Type_Should_Fail() {
      var result = Check(typeof(WrongSelfType));
      Assert.AreEqual(false, result.Success);
      Assert.AreEqual(1, result.Messages.Count());
      var error = result.Messages.First();
      Assert.AreEqual((int)Error.Code.SelfTypeConstraintNotSetToCompositionType, error.Number);
      Assert.AreEqual(
        string.Format(
          "Composition '{0}' doesn't provide itself as the self-type parameter to role '{1}'. It uses 'System.String' instead.",
          GetType<WrongSelfType>(), GetType(typeof(RSelfType<>))), 
        error.Text);
    }
    class WrongSelfType : Does<RSelfType<string>> { }

    [Test]
    public void Test_Right_Self_Type_Type_Argument_Should_Succeed() {
      var result = Check(typeof(RightSelfType<>));
      Assert.AreEqual(true, result.Success);
    }
    class RightSelfType<T> : Does<RSelfType<RightSelfType<T>>> { }

    [Test]
    public void Test_Wrong_Self_Type_Type_Argument_Should_Fail() {
      var result = Check(typeof(WrongSelfType<>));
      Assert.AreEqual(false, result.Success);
      Assert.AreEqual(1, result.Messages.Count());
    }
    class WrongSelfType<T> : Does<RSelfType<WrongSelfType<string>>> { }

    [Test]
    public void Test_Self_Type_Switched_Type_Arguments_Should_Fail() {
      var result = Check(typeof(SwitchedSelfType<,>));
      Assert.AreEqual(false, result.Success);
      Assert.AreEqual(1, result.Messages.Count());
    }
    class SwitchedSelfType<T, U> : Does<RSelfType<SwitchedSelfType<U, T>>> { }

    [Test]
    public void Test_Self_Type_Nested_Right_Type_Argument_Should_Succeed() {
      var result = Check(typeof(Container<>.RightSelfType<>));
      Assert.AreEqual(true, result.Success);
    }
    partial class Container<T> {
      public class RightSelfType<U> : Does<RSelfType<RightSelfType<U>>> { }
    }

    [Test]
    public void Test_Self_Type_Nested_Wrong_Type_Argument_Should_Fail() {
      var result = Check(typeof(Container<>.WrongSelfType<>));
      Assert.AreEqual(false, result.Success);
      Assert.AreEqual(1, result.Messages.Count());
    }
    partial class Container<T> {
      public class WrongSelfType<U> : Does<RSelfType<WrongSelfType<T>>> { }
    }

    [Test]
    public void Test_Self_Type_With_Global_Type_Argument_Should_Fail() {
      var result = Check(typeof(GlobalTSelfType<>));
      Assert.AreEqual(false, result.Success);
      Assert.AreEqual(1, result.Messages.Count());
    }
    class GlobalTSelfType<T> : Does<RSelfType<GlobalTSelfType<global::T>>> { }

  }

}

class T { }
