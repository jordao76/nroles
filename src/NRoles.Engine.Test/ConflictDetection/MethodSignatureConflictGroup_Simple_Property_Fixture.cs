using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.ConflictDetection {
  
  [TestFixture]
  public class MethodSignatureConflictGroup_Simple_Property_Fixture : AssemblyReadonlyFixture {

    RoleCompositionMember _member;
    IConflictGroup _group;

    class Class_With_Property { public int MyProperty { get; set; } }

    [SetUp]
    public void SetUp() {
      var targetType = GetType<Class_With_Property>();
      _member = new RoleMember(targetType, targetType.Properties[0]);
      _group = new MethodSignatureConflictGroup();
    }

    [Test]
    public void Property_Should_Not_Be_Accepted_By_Group() {
      Assert.AreEqual(false, _group.Matches(_member));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Property_Added_To_Group_Should_Throw() {
      _group.AddMember(_member);
    }

  }

}
