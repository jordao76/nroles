using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test.ConflictDetection {

  [TestFixture]
  public class ConflictDetectorFixture : AssemblyReadonlyFixture {

    class Empty { }
    [Test]
    public void Test_No_Roles_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process();
      OperationAssert.IsSuccessful(result);
    }

    // TODO: having to declare Code is awkward! Make it a mockable strategy in the conflict detector!
    //   we need it to find out if the role methods are abstract, do we need it for anything else?
    interface Empty_Role : Role { }
    [Test]
    public void Test_Empty_Role_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Empty_Role>());
      OperationAssert.IsSuccessful(result);
    }

    interface Empty_Role_1 : Role { }
    interface Empty_Role_2 : Role { }
    [Test]
    public void Test_Two_Empty_Roles_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Empty_Role_1>(), GetType<Empty_Role_2>());
      OperationAssert.IsSuccessful(result);
    }

    interface Role_With_Method : Role { void Method(); } 
    class Role_With_Method_Code { static void Method(Role_With_Method p) { } }
    [Test]
    public void Test_Role_With_Method_In_Empty_Class_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method>());
      OperationAssert.IsSuccessful(result);
    }

    class Class_With_Method { public void Method() { } }
    [Test]
    public void Test_Superseded_Role_Method_No_Conflict() {
      var targetType = GetType<Class_With_Method>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method>());
      OperationAssert.IsSuccessful(result);
      // TODO: check for warning that the method is not marked [Supersede] in the class?
      // TODO: check that the method in the group is really superseded!
    }

    interface Role_With_Method1 : Role { void Method1(); }
    class Role_With_Method1_Code { static void Method1(Role_With_Method1 p) { } }
    interface Role_With_Method2 : Role { void Method2(); }
    class Role_With_Method2_Code { static void Method2(Role_With_Method2 p) { } }
    [Test]
    public void Test_Two_Roles_No_Conflicts() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method1>(), GetType<Role_With_Method2>());
      OperationAssert.IsSuccessful(result);
    }

    interface Role_With_Method_2 : Role { void Method(); }
    class Role_With_Method_2_Code { static void Method(Role_With_Method_2 p) { } }
    [Test]
    public void Test_Two_Roles_With_Conflicting_Method() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method>(), GetType<Role_With_Method_2>());
      OperationAssert.Failed(result);
    }

    interface Role_With_Method_Take_Int32 : Role { void Method(int p); }
    class Role_With_Method_Take_Int32_Code { static void Method(Role_With_Method_Take_Int32 p, int q) { } }
    interface Role_With_Method_Take_String : Role { void Method(string p); }
    class Role_With_Method_Take_String_Code { static void Method(Role_With_Method_Take_String p, string q) { } }
    [Test]
    public void Test_Two_Roles_With_Overloaded_Method_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method_Take_Int32>(), GetType<Role_With_Method_Take_String>());
      OperationAssert.IsSuccessful(result);
    }


    class Class_With_Method_Take_String { public void Method(string p) { } }
    [Test]
    public void Test_Role_Method_Overloads_Class_Method_No_Conflict() {
      var targetType = GetType<Class_With_Method_Take_String>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method_Take_Int32>());
      OperationAssert.IsSuccessful(result);
    }

    interface Role_With_Method_Return_Int32 : Role { int Method(); }
    class Role_With_Method_Return_Int32_Code { static int Method(Role_With_Method_Return_Int32 p) { return 0; } }
    interface Role_With_Method_Return_String : Role { string Method(); }
    class Role_With_Method_Return_String_Code { static string Method(Role_With_Method_Return_String p) { return ""; } }
    [Test]
    public void Test_Two_Roles_With_Methods_That_Differ_On_Return_Type_Should_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method_Return_Int32>(), GetType<Role_With_Method_Return_String>());
      OperationAssert.Failed(result);
      var messages = result.Messages.ToList();
      Assert.AreEqual(1, messages.Count);
      Assert.AreEqual((int)Error.Code.MethodsWithConflictingSignatures, messages[0].Number);
      // TODO: add the class' members to the groups
      // TODO: this message is also valid for when the members differ in accessibility?
    }

    interface Role_With_Property_Named_Method : Role { int Method { get; } }
    class Role_With_Property_Named_Method_Code { static int get_Method(Role_With_Property_Named_Method p) { return 0; } }
    [Test]
    public void Test_Two_Roles_With_Members_With_The_Same_Name_Should_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method_Take_String>(), GetType<Role_With_Property_Named_Method>());
      OperationAssert.Failed(result);
      var messages = result.Messages.ToList();
      Assert.AreEqual(1, messages.Count);
      Assert.AreEqual((int)Error.Code.MembersWithSameName, messages[0].Number);
    }

  }

}
