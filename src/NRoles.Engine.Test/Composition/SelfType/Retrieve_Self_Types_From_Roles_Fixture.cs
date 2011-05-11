using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test.Composition.SelfType {

  [TestFixture]
  public class Retrieve_Self_Types_From_Roles_Fixture : AssemblyReadonlyFixture {

    private IEnumerable<RoleSelfType> GetRolesAndSelfTypes<C>() {
      var source = GetType<C>();
      var extractor = new SelfTypeExtractor();
      return extractor.RetrieveRolesSelfTypes(source);
    }

    [Test]
    public void Test_No_Self_Types_For_Role_Without_Self_Type() { 
      var rolesAndSelfTypes = GetRolesAndSelfTypes<NoSelfType>();
      Assert.AreEqual(0, rolesAndSelfTypes.Count());
    }
    class RNoSelfType : Role { }
    class NoSelfType : Does<RNoSelfType> { }

    private void AssertRoleSelfType(RoleSelfType roleSelfType, Type roleType, Type sourceType) {
      var role = GetType(roleType);
      var source = GetType(sourceType);
      Assert.AreEqual(role.ToString(), roleSelfType.Role.Resolve().ToString());
      Assert.AreEqual(source.ToString(), ((GenericInstanceType)roleSelfType.Role).GenericArguments[0].FullName);
      Assert.AreEqual(source.ToString(), roleSelfType.SelfType.ToString());
    }

    [Test]
    public void Test_Self_Type_For_Composition_With_One_Role() {
      var rolesAndSelfTypes = GetRolesAndSelfTypes<SelfType>();
      Assert.AreEqual(1, rolesAndSelfTypes.Count());
      AssertRoleSelfType(rolesAndSelfTypes.First(), typeof(RSelfType<>), typeof(SelfType));
    }
    class RSelfType<TSelf> : Role { }
    class SelfType : Does<RSelfType<SelfType>> { }

    [Test]
    public void Test_Self_Types_For_Composition_With_Two_Roles() {
      var rolesAndSelfTypes = GetRolesAndSelfTypes<TwoSelfTypes>();
      Assert.AreEqual(2, rolesAndSelfTypes.Count());
      AssertRoleSelfType(rolesAndSelfTypes.First(), typeof(RSelfType<>), typeof(TwoSelfTypes));
      AssertRoleSelfType(rolesAndSelfTypes.Last(), typeof(RAnotherSelfType<>), typeof(TwoSelfTypes));
    }
    class RAnotherSelfType<TSelf> : Role { }
    class TwoSelfTypes : Does<RSelfType<TwoSelfTypes>>, Does<RAnotherSelfType<TwoSelfTypes>> { }

  }

}
