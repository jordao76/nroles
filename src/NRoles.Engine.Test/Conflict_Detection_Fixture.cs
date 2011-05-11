using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {

  [TestFixture]
  public class Conflict_Detection_Fixture : AssemblyReadonlyFixture {

    class Empty { }
    [Test]
    public void Test_No_Roles_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process();
      Assert.IsTrue(result.Success);
    }

    // TODO: having to declare Code is awkward! Make it a mockable strategy in the conflict detector!
    //   we need it to find out if the role methods are abstract, do we need it for anything else?
    class Empty_Role { class Code { } } // no need to mark with the Role interface
    [Test]
    public void Test_Empty_Role_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Empty_Role>());
      Assert.IsTrue(result.Success);
    }

    class Empty_Role_1 { class Code { } }
    class Empty_Role_2 { class Code { } }
    [Test]
    public void Test_Two_Empty_Roles_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Empty_Role_1>(), GetType<Empty_Role_2>());
      Assert.IsTrue(result.Success);
    }

    class Role_With_Method { public void Method() { } class Code { static void Method(Role_With_Method p) { } } }
    [Test]
    public void Test_Role_With_Method_In_Empty_Class_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method>());
      Assert.IsTrue(result.Success);
    }

    class Class_With_Method { public void Method() { } }
    [Test]
    public void Test_Superseded_Role_Method_No_Conflict() {
      var targetType = GetType<Class_With_Method>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method>());
      Assert.IsTrue(result.Success);
      // TODO: check for warning that the method is not marked [Supersede] in the class?
      // TODO: check that the method in the group is really superseded!
    }

    class Role_With_Method1 { public void Method1() { } class Code { static void Method1(Role_With_Method1 p) { } } }
    class Role_With_Method2 { public void Method2() { } class Code { static void Method2(Role_With_Method2 p) { } } }
    [Test]
    public void Test_Two_Roles_No_Conflicts() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method1>(), GetType<Role_With_Method2>());
      Assert.IsTrue(result.Success);
    }

    class Role_With_Method_2 { public void Method() { } class Code { static void Method(Role_With_Method_2 p) { } } }
    [Test]
    public void Test_Two_Roles_With_Conflicting_Method() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method>(), GetType<Role_With_Method_2>());
      Assert.IsFalse(result.Success);
    }

    class Role_With_Method_Take_Int32 { public void Method(int p) { } class Code { static void Method(Role_With_Method_Take_Int32 p, int q) { } } }
    class Role_With_Method_Take_String { public void Method(string p) { } class Code { static void Method(Role_With_Method_Take_String p, string q) { } } }
    [Test]
    public void Test_Two_Roles_With_Overloaded_Method_No_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method_Take_Int32>(), GetType<Role_With_Method_Take_String>());
      Assert.IsTrue(result.Success);
    }


    class Class_With_Method_Take_String { public void Method(string p) { } }
    [Test]
    public void Test_Role_Method_Overloads_Class_Method_No_Conflict() {
      var targetType = GetType<Class_With_Method_Take_String>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method_Take_Int32>());
      Assert.IsTrue(result.Success);
    }


    class Role_With_Method_Return_Int32 { public int Method() { return 0; } class Code { static int Method(Role_With_Method_Return_Int32 p) { return 0; } } }
    class Role_With_Method_Return_String { public string Method() { return ""; } class Code { static string Method(Role_With_Method_Return_String p) { return ""; } } }
    [Test]
    public void Test_Two_Roles_With_Methods_That_Differ_On_Return_Type_Should_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method_Return_Int32>(), GetType<Role_With_Method_Return_String>());
      Assert.IsFalse(result.Success);
      var messages = result.Messages.ToList();
      Assert.AreEqual(1, messages.Count);
      Assert.AreEqual(45, messages[0].Number);
      // TODO: add the class' members to the groups
      // TODO: this message is also valid for when the members differ in accessibility?
    }

    class Role_With_Property_Named_Method { public int Method { get { return 0; } } class Code { static int get_Method(Role_With_Property_Named_Method p) { return 0; } } }
    [Test]
    public void Test_Two_Roles_With_Members_With_The_Same_Name_Should_Conflict() {
      var targetType = GetType<Empty>();
      var detector = new ConflictDetector(targetType);
      var result = detector.Process(GetType<Role_With_Method_Take_String>(), GetType<Role_With_Property_Named_Method>());
      Assert.IsFalse(result.Success);
      var messages = result.Messages.ToList();
      Assert.AreEqual(1, messages.Count);
      Assert.AreEqual(50, messages[0].Number);
    }

  }

}
