using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test.ConflictDetection {
  
  [TestFixture]
  public class ContributedConflictGroup_Generic_Methods_Fixture : AssemblyReadonlyFixture {

    IConflictGroup _group; 

    class Empty { }
    class Generic<T> { public void Method(T p) { } }
    class Inherited : Generic<int> { }
    class NonGeneric { public void Method(int p) { } }

    private new RoleCompositionMember GetMethodByName(Type t, string methodName) {
      var classMethod = base.GetMethodByName(t, methodName);
      return new RoleMember(classMethod.Class, classMethod.Definition);
    }

    [SetUp]
    public void SetUp() {
      var targetType = GetType(typeof(Empty));
      _group = new ContributedConflictGroup(targetType);
    }

    [Test]
    public void Generic_Method_With_Type_Argument_Should_Match_Equivalent_Non_Generic_Method() {
      var roleMethod1 = GetMethodByName(typeof(Inherited), "Method");
      _group.AddMember(roleMethod1);
      var roleMethod2 = GetMethodByName(typeof(NonGeneric), "Method");
      Assert.AreEqual(true, _group.Matches(roleMethod2));
    }

    [Test]
    public void Generic_Method_Should_Match_Generic_Method_With_Type_Argument() {
      var roleMethod1 = GetMethodByName(typeof(Generic<>), "Method");
      _group.AddMember(roleMethod1);
      var roleMethod2 = GetMethodByName(typeof(Inherited), "Method");
      Assert.AreEqual(true, _group.Matches(roleMethod2));
    }
  
  }
}
