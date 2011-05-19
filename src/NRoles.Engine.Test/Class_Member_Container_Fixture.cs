using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {

  // TODO: move to its own file
  public abstract class AssemblyReadonlyFixture {
    static AssemblyAccessor _assembly = new AssemblyAccessor();
    static AssemblyReadonlyFixture() {
      new MutationContext(((AssemblyDefinition)_assembly).MainModule);
    }
    protected TypeDefinition GetType<T>() {
      return _assembly.GetType<T>();
    }
    protected TypeDefinition GetType(Type type) {
      return _assembly.GetType(type);
    }
  }

  [TestFixture]
  public class Class_Member_Container_Fixture : AssemblyReadonlyFixture {

    class Class_With_Method { public void Method() { } }
    [Test]
    public void Test_Method_Should_Be_Present() {
      var container = new ClassMemberContainer(GetType<Class_With_Method>());
      var members = container.Members;
      Assert.That(members.Any(member => member.Definition.Name == "Method" && !member.IsInherited));
    }

    class Empty { }
    [Test]
    public void Test_Object_Members_Should_Be_Present() {
      var container = new ClassMemberContainer(GetType<Empty>());
      var members = container.Members;
      Assert.That(members.Any(member => member.Definition.Name == "ToString" && member.IsInherited));
      Assert.That(members.Any(member => member.Definition.Name == "Equals" && member.IsInherited));
      Assert.That(members.Any(member => member.Definition.Name == "GetHashCode" && member.IsInherited));
      Assert.That(members.Any(member => member.Definition.Name == "GetType" && member.IsInherited));
    }

    class Overrides_ToString { public override string ToString() { return ""; } }
    [Test]
    public void Test_Overridden_Method_Should_Not_Be_Present() {
      var container = new ClassMemberContainer(GetType<Overrides_ToString>());
      
      var overridden = container.Members.SingleOrDefault(member =>
        member.Definition.Name == "ToString" &&
        member.Class.Name == "Object");
      Assert.IsNull(overridden);

      var overriding = container.Members.Single(member =>
        member.Definition.Name == "ToString" &&
        member.Class.Name == "Overrides_ToString" && 
        !member.IsInherited);
      Assert.IsNotNull(overriding);
    }

    class Shadows_ToString { public new string ToString() { return ""; } }
    [Test]
    public void Test_Shadowed_Method_Should_Not_Be_Present() {
      var container = new ClassMemberContainer(GetType<Shadows_ToString>());

      var shadowed = container.Members.SingleOrDefault(member =>
        member.Definition.Name == "ToString" &&
        member.Class.Name == "Object");
      Assert.IsNull(shadowed);

      var shadowing = container.Members.Single(member =>
        member.Definition.Name == "ToString" &&
        member.Class.Name == "Shadows_ToString" &&
        !member.IsInherited);
      Assert.IsNotNull(shadowing);
    }

    class Class_With_Private_Method { void Method() { } }
    [Test] public void Test_Private_Method_Should_Be_Present() {
      var container = new ClassMemberContainer(GetType<Class_With_Private_Method>());
      var method = container.Members.Single(member =>
        member.Definition.Name == "Method" && !member.IsInherited);
      Assert.IsNotNull(method);
    }

    class Derived_From_Class_With_Private_Method : Class_With_Private_Method { }
    [Test] public void Test_Private_Method_From_Base_Class_Should_Not_Be_Present() {
      var container = new ClassMemberContainer(GetType<Derived_From_Class_With_Private_Method>());
      var method = container.Members.SingleOrDefault(member =>
        member.Definition.Name == "Method");
      Assert.IsNull(method);
    }

    class Generic<T> { public void Method(T t) { } }
    class Shadows_Generic_With_Non_Generic : Generic<int> { public new void Method(int i) { } }
    [Test]
    public void Test_Shadowed_Generic_Method_Should_Not_Be_Present() {
      var container = new ClassMemberContainer(GetType<Shadows_Generic_With_Non_Generic>());

      var shadowed = container.Members.SingleOrDefault(member =>
        member.Definition.Name == "Method" &&
        member.Class.Name == "Generic`1");
      Assert.IsNull(shadowed);

      var shadowing = container.Members.Single(member =>
        member.Definition.Name == "Method" &&
        member.Class.Name == "Shadows_Generic_With_Non_Generic" &&
        !member.IsInherited);
      Assert.IsNotNull(shadowing);
    }

    class Inherits_From_Generic : Generic<int> { }
    [Test]
    public void Test_Class_Inherited_From_Generic_Should_Contain_The_Parent_Generic_Method() {
      var container = new ClassMemberContainer(GetType<Inherits_From_Generic>());

      var method = container.Members.Single(member =>
        member.Definition.Name == "Method");
      Assert.IsNotNull(method);

      Assert.AreEqual("Generic`1", method.Class.Name);
      Assert.IsTrue(method.Class is GenericInstanceType);
      var genericClass = (GenericInstanceType)method.Class;
      Assert.AreEqual("Int32", genericClass.GenericArguments[0].Name);
    }

    class Generic2<T> : Generic<int> { public void Method(T t) { } }
    class NonGeneric : Generic2<string> { }
    [Test]
    public void Test_Generic_Hierarchy_Should_Contain_The_Right_Methods() {
      var container = new ClassMemberContainer(GetType<NonGeneric>());

      var methods = container.Members.Where(member =>
        member.Definition.Name == "Method").ToList();
      Assert.AreEqual(2, methods.Count);
    }

    // TODO: 
    // static => conflicts with other members in C#
    // generics methods, 
    // explicit iterface implementations, 
    // nested => conflicts with other members in C#

  }

}
