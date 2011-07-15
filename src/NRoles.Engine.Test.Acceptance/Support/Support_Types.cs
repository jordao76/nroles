using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support {

  // TODO: think about using a composition as a Role in another assembly; it will end up "implementing" Role through the roles it composes

  [RoleTest(
    CompositionType = typeof(Empty_Role_Composition),
    TestType = typeof(Empty_Role_Composition_Test))]
  public class Empty_Role : Role { }
  public class Empty_Role_Composition : Does<Empty_Role> { }
  public class Empty_Role_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Empty_Role_Composition().As<Empty_Role>();
      Assert.IsNotNull(role);
    }
  }

  [RoleTest(
    CompositionType = typeof(Empty_Interface_Role_Composition),
    TestType = typeof(Empty_Interface_Role_Composition_Test))]
  public interface Empty_Interface_Role : Role { }
  public class Empty_Interface_Role_Composition : Does<Empty_Interface_Role> { }
  public class Empty_Interface_Role_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Empty_Interface_Role_Composition().As<Empty_Interface_Role>();
      Assert.IsNotNull(role);
    }
  }

  [RoleTest(
    OtherRoles = new Type[] { typeof(Empty_Role) },
    CompositionType = typeof(Two_Empty_Roles_Composition),
    TestType = typeof(Two_Empty_Roles_Composition_Test))]
  public class Another_Empty_Role : Role { }
  public class Two_Empty_Roles_Composition : Does<Empty_Role>, Does<Another_Empty_Role> { }
  public class Two_Empty_Roles_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Two_Empty_Roles_Composition().As<Empty_Role>();
      Assert.IsNotNull(role);
      var otherRole = role.Cast<Another_Empty_Role>();
      Assert.IsNotNull(otherRole);
      var newRole = new Two_Empty_Roles_Composition().As<Another_Empty_Role>();
      Assert.IsNotNull(newRole);
    }
  }

  [RoleTest(
    CompositionType = typeof(Generic_Empty_Role_Composition<>),
    TestType = typeof(Generic_Empty_Role_Test))]
  [RoleTest(CompositionType = typeof(Generic_Empty_Role_Composition_String))]
  public class Generic_Empty_Role<T> : Role { }
  public class Generic_Empty_Role_Composition<U> : Does<Generic_Empty_Role<U>> { }
  public class Generic_Empty_Role_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Generic_Empty_Role_Composition<int>().As<Generic_Empty_Role<int>>();
      Assert.IsNotNull(role);
    }
  }
  public class Generic_Empty_Role_Composition_String : Does<Generic_Empty_Role<string>> { }

  [RoleTest(CompositionType = typeof(Doubly_Generic_Empty_Role_Composition<,>))]
  [RoleTest(CompositionType = typeof(Doubly_Generic_Empty_Role_Composition_Left<>))]
  [RoleTest(CompositionType = typeof(Doubly_Generic_Empty_Role_Composition_Right<>))]
  [RoleTest(CompositionType = typeof(Doubly_Generic_Empty_Role_Composition_Full))]
  public class Doubly_Generic_Empty_Role<T, U> : Role { }
  public class Doubly_Generic_Empty_Role_Composition<T, U> : Does<Doubly_Generic_Empty_Role<T, U>> { }
  public class Doubly_Generic_Empty_Role_Composition_Left<T> : Does<Doubly_Generic_Empty_Role<string, T>> { }
  public class Doubly_Generic_Empty_Role_Composition_Right<T> : Does<Doubly_Generic_Empty_Role<T, string>> { }
  public class Doubly_Generic_Empty_Role_Composition_Full : Does<Doubly_Generic_Empty_Role<int, string>> { }

  [RoleTest(
    CompositionType = typeof(Role_With_Method_Composition),
    TestType = typeof(Role_With_Method_Composition_Test))]
  public class Role_With_Method : Role {
    public IEnumerable<string> Reverse(IEnumerable<string> list) {
      foreach (string str in list.Reverse()) {
        yield return str;
      }
    }
  }
  public class Role_With_Method_Composition : Does<Role_With_Method> { }
  public class Role_With_Method_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Method_Composition().As<Role_With_Method>();
      Assert.IsNotNull(role);
      var result = role.Reverse(new string[] { "night", "wish" });
      Assert.IsNotNull(result);
      Assert.AreEqual(2, result.Count());
      Assert.AreEqual("wish", result.First());
      Assert.AreEqual("night", result.Skip(1).First());
    }
  }

  [RoleTest(CompositionType = typeof(Generic_Role_With_Method_Composition<>))]
  [RoleTest(CompositionType = typeof(Generic_Role_With_Method_Composition_String))]
  public class Generic_Role_With_Method<T> : Role {
    public IEnumerable<T> Method(IEnumerable<T> element) {
      return null;
    }
  }
  public class Generic_Role_With_Method_Composition<U> : Does<Generic_Role_With_Method<U>> { }
  public class Generic_Role_With_Method_Composition_String : Does<Generic_Role_With_Method<string>> { }

  [RoleTest]
  public class Role_With_Private_Field : Role {
    private string _field = "Test";
    public string Property { get { return _field; } set { _field = value; } }
    public void Method1(string value) { _field = value; }
    public string Method2() { return _field; }
  }

  [RoleTest]
  public class Generic_Role_With_Private_Field<T> : Role {
    private T _field;
    public T Property { get { return _field; } set { _field = value; } }
    public void Method1(T value) { _field = value; }
    public T Method2() { return _field; }
  }

  // TODO: role with protected field!

  [RoleTest(CompositionType = typeof(Simple_Role_With_Generic_Method_Composition))]
  public class Simple_Role_With_Generic_Method : Role {
    public void Method<T>(T arg) { }
  }
  public class Simple_Role_With_Generic_Method_Composition : Does<Simple_Role_With_Generic_Method> { }

  [RoleTest(CompositionType = typeof(Generic_Role_With_Generic_Method_Composition<>))]
  [RoleTest(CompositionType = typeof(Generic_Role_With_Generic_Method_NonGeneric_Composition))]
  public class Generic_Role_With_Generic_Method<U> : Role {
    public void Method<T>(T arg, U arg2) { }
  }
  public class Generic_Role_With_Generic_Method_Composition<T> : Does<Generic_Role_With_Generic_Method<T>> { }
  public class Generic_Role_With_Generic_Method_NonGeneric_Composition : Does<Generic_Role_With_Generic_Method<int>> { }

  [RoleTest(
    CompositionType = typeof(Role_With_Generic_Method_Composition),
    TestType = typeof(Role_With_Generic_Method_Test))]
  public class Role_With_Generic_Method : Role {
    public T First<T>(IEnumerable<T> element) {
      return element.First();
    }
  }
  public class Role_With_Generic_Method_Composition : Does<Role_With_Generic_Method> { }
  public class Role_With_Generic_Method_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Generic_Method_Composition().As<Role_With_Generic_Method>();
      string first = role.First(new string[] { "Hello", "World" });
      Assert.AreEqual("Hello", first);
    }
  }

  [RoleTest(CompositionType = typeof(Role_With_Overloaded_Method_Composition))]
  public class Role_With_Overloaded_Method : Role {
    public void Method(int p) { }
    public void Method(double p) { }
  }
  public class Role_With_Overloaded_Method_Composition : Does<Role_With_Overloaded_Method> { }

  [RoleTest(
    CompositionType = typeof(Role_With_Abstract_Method_Composition),
    TestType = typeof(Role_With_Abstract_Method_Composition_Test))]
  [RoleTest(
    CompositionType = typeof(Role_With_Abstract_Method_Composition_Without_The_Method),
    ExpectedCompositionError = Error.Code.DoesNotImplementAbstractRoleMember)]
  [RoleTest(
    CompositionType = typeof(Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_A_Base_Class),
    TestType = typeof(Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_A_Base_Class_Test))]

  public abstract class Role_With_Abstract_Method : Role {
    public abstract int Method();
  }
  public class Role_With_Abstract_Method_Composition : Does<Role_With_Abstract_Method> {
    // TODO: needs [Supercede] for abstracts?
    public int Method() { return 42; }
  }
  public class Role_With_Abstract_Method_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Abstract_Method_Composition().As<Role_With_Abstract_Method>();
      Assert.AreEqual(42, role.Method());
    }
  }
  public abstract class Role_With_Abstract_Method_Composition_Without_The_Method : Does<Role_With_Abstract_Method> { }

  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_With_Abstract_Method) },
    CompositionType = typeof(Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_Another_Role),
    TestType = typeof(Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_Another_Role_Test))]
  public class Role_With_Expected_Abstract_Method : Role {
    public int Method() { return 44; }
  }
  public class Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_Another_Role :
    Does<Role_With_Abstract_Method>, Does<Role_With_Expected_Abstract_Method> { }
  public class Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_Another_Role_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_Another_Role().As<Role_With_Abstract_Method>();
      Assert.AreEqual(44, role.Method());
    }
  }

  public class Class_With_Expected_Abstract_Method {
    public int Method() { return 78; }
  }
  public class Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_A_Base_Class :
    Class_With_Expected_Abstract_Method, Does<Role_With_Abstract_Method> { }
  public class Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_A_Base_Class_Test : DynamicTestFixture {
    public override void Test() {
      var composition = new Role_With_Abstract_Method_Composition_Where_The_Method_Comes_From_A_Base_Class();
      Assert.AreEqual(78, composition.Method());
      var role = composition.As<Role_With_Abstract_Method>();
      Assert.AreEqual(78, role.Method());
    }
  }

  [RoleTest(
    CompositionType = typeof(Disposable_Role_Composition),
    TestType = typeof(Disposable_Role_Composition_Test))]
  public class Disposable_Role : Role, IDisposable {
    bool _disposed = false;
    public bool Disposed {
      get { return _disposed; }
    }
    public void Dispose() {
      _disposed = true;
    }
  }
  public class Disposable_Role_Composition : Does<Disposable_Role> { }
  public class Disposable_Role_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Disposable_Role_Composition().As<Disposable_Role>();
      using (role) {
        Assert.AreEqual(false, role.Disposed);
      }
      Assert.AreEqual(true, role.Disposed);
    }
  }

  [RoleTest(
    CompositionType = typeof(Role_With_Public_Field_Composition),
    TestType = typeof(Role_With_Public_Field_Composition_Test))]
  public class Role_With_Public_Field : Role {
    public string Field = "Hello";
  }
  public class Role_With_Public_Field_Composition : Does<Role_With_Public_Field> { }
  public class Role_With_Public_Field_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_With_Public_Field_Composition().As<Role_With_Public_Field>();
      Assert.AreEqual("Hello", role.Field);
      role.Field += " World";
      Assert.AreEqual("Hello World", role.Field);
    }
  }

  [RoleTest(
    CompositionType = typeof(Generic_Role_With_Public_Field_Composition<>),
    TestType = typeof(Generic_Role_With_Public_Field_Composition_Test))]
  [RoleTest(
    CompositionType = typeof(Generic_Role_With_Public_Field_Composition_String),
    TestType = typeof(Generic_Role_With_Public_Field_Composition_String_Test))]
  public class Generic_Role_With_Public_Field<T> : Role {
    public T Field;
  }
  public class Generic_Role_With_Public_Field_Composition<T> : Does<Generic_Role_With_Public_Field<T>> { }
  public class Generic_Role_With_Public_Field_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Generic_Role_With_Public_Field_Composition<string>().As<Generic_Role_With_Public_Field<string>>();
      role.Field = "Hello";
      Assert.AreEqual("Hello", role.Field);
      role.Field += " World";
      Assert.AreEqual("Hello World", role.Field);
    }
  }
  public class Generic_Role_With_Public_Field_Composition_String : Does<Generic_Role_With_Public_Field<string>> { }
  public class Generic_Role_With_Public_Field_Composition_String_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Generic_Role_With_Public_Field_Composition_String().As<Generic_Role_With_Public_Field<string>>();
      role.Field = "Hello";
      Assert.AreEqual("Hello", role.Field);
      role.Field += " World";
      Assert.AreEqual("Hello World", role.Field);
    }
  }

  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Used_In_Other_Role) },
    CompositionType = typeof(Role_That_Uses_Another_Role_Composition),
    TestType = typeof(Role_That_Uses_Another_Role_Composition_Test))]
  public class Role_That_Uses_Another_Role : Role, Does<Role_Used_In_Other_Role> { }
  public class Role_Used_In_Other_Role : Role {
    public int Method() { return 42; }
  }
  public class Role_That_Uses_Another_Role_Composition : Does<Role_That_Uses_Another_Role> { }
  public class Role_That_Uses_Another_Role_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_That_Uses_Another_Role_Composition().
        As<Role_That_Uses_Another_Role>().
        As<Role_Used_In_Other_Role>();
      Assert.AreEqual(42, role.Method());
      // TODO: a role that uses another role does not inherit from the other role interface!!! check this in the test, use a role from another assembly!
    }
  }

  [RoleTest(
    OtherRoles = new Type[] { typeof(Generic_Role_Used_In_Other_Role<,,>) },
    CompositionType = typeof(Generic_Role_That_Uses_Another_Role_Composition<>),
    TestType = typeof(Generic_Role_That_Uses_Another_Role_Composition_Test))]
  public class Generic_Role_That_Uses_Another_Role<T, U> : Role, Does<Generic_Role_Used_In_Other_Role<T, U, T>> { }
  public class Generic_Role_Used_In_Other_Role<T, U, V> : Role {
    public T MethodT(T param) { return param; }
    public U MethodU(U param) { return param; }
    public V MethodV(V param) { return param; }
  }
  public class Generic_Role_That_Uses_Another_Role_Composition<T> : Does<Generic_Role_That_Uses_Another_Role<T, string>> { }
  public class Generic_Role_That_Uses_Another_Role_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Generic_Role_That_Uses_Another_Role_Composition<int>().
        As<Generic_Role_That_Uses_Another_Role<int, string>>().
        As<Generic_Role_Used_In_Other_Role<int, string, int>>();
      Assert.AreEqual(42, role.MethodT(42));
      Assert.AreEqual("Don't Panic", role.MethodU("Don't Panic"));
      Assert.AreEqual(28, role.MethodV(28));
    }
  }

  [RoleTest(ExpectedRoleError = Error.Code.RoleInheritsFromClass)]
  public class Role_With_Base_Class : Base_Class, Role { }
  public class Base_Class { }

  #region Conflicts

  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Method_Conflict_2) },
    CompositionType = typeof(Role_Method_Conflict_Composition),
    ExpectedCompositionError = Error.Code.Conflict)]
  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Method_Conflict_2) },
    CompositionType = typeof(Role_Method_Conflict_Resolved_Through_Exclusion_Composition),
    TestType = typeof(Role_Method_Conflict_Resolved_Through_Exclusion_Composition_Test))]
  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Method_Conflict_2) },
    CompositionType = typeof(Role_Method_Conflict_View_Method_With_Wrong_Return_Type),
    ExpectedCompositionError = Error.Code.RoleViewMemberNotFoundInRole)]
  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Method_Conflict_2) },
    CompositionType = typeof(Role_Method_Conflict_Resolved_Through_Aliasing_Composition),
    TestType = typeof(Role_Method_Conflict_Resolved_Through_Aliasing_Composition_Test))]
  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Method_Conflict_2) },
    CompositionType = typeof(Role_Method_Conflict_Exclude_Too_Much_Composition),
    ExpectedCompositionError = Error.Code.AllMembersExcluded)]
  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Method_Conflict_2) },
    CompositionType = typeof(Role_Method_Conflict_Composition_View_With_Multiple_Roles),
    ExpectedCompositionError = Error.Code.RoleViewWithMultipleRoles)]
  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Method_Conflict_2) },
    CompositionType = typeof(Role_Method_Conflict_Alias_Both_Composition),
    TestType = typeof(Role_Method_Conflict_Alias_Both_Composition_Test))]
  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_Method_Conflict_2) },
    CompositionType = typeof(Role_Method_Alias_Conflict_Composition),
    ExpectedCompositionError = Error.Code.Conflict)]

  public class Role_Method_Conflict_1 : Role { public int Method() { return 1; } }
  public class Role_Method_Conflict_2 : Role { public int Method() { return 2; } }

  public class Role_Method_Conflict_Composition : Does<Role_Method_Conflict_1>, Does<Role_Method_Conflict_2> { }

  public class Role_Method_Conflict_Resolved_Through_Exclusion_Composition : Does<Role_Method_Conflict_1>, Does<Role_Method_Conflict_2>, Does<Role_Method_Conflict_Resolved_Through_Exclusion_Composition.View> {
    interface View : RoleView<Role_Method_Conflict_1> { [Exclude] int Method(); }
  }
  public class Role_Method_Conflict_Resolved_Through_Exclusion_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_Method_Conflict_Resolved_Through_Exclusion_Composition().As<Role_Method_Conflict_1>();
      Assert.AreEqual(2, role.Method()); // since Role_Method_Conflict_1::Method was excluded, this calls Role_Method_Conflict_2::Method
      var otherRole = role.Cast<Role_Method_Conflict_2>();
      Assert.AreEqual(2, role.Method());

      // TODO: reflection asserts!
    }
  }

  public class Role_Method_Conflict_View_Method_With_Wrong_Return_Type : Does<Role_Method_Conflict_1>, Does<Role_Method_Conflict_2>, Does<Role_Method_Conflict_View_Method_With_Wrong_Return_Type.View> {
    interface View : RoleView<Role_Method_Conflict_1> { 
      [Exclude] void Method(); // in the role, the method returns an int
    }
  }

  public class Role_Method_Conflict_Resolved_Through_Aliasing_Composition : Does<Role_Method_Conflict_1>, Does<Role_Method_Conflict_2>, Does<Role_Method_Conflict_Resolved_Through_Aliasing_Composition.View> {
    public interface View : RoleView<Role_Method_Conflict_1> { [Aliasing("Method")] int AliasedMethod(); }
  }
  public class Role_Method_Conflict_Resolved_Through_Aliasing_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Role_Method_Conflict_Resolved_Through_Aliasing_Composition().As<Role_Method_Conflict_1>();
      Assert.AreEqual(1, role.Method()); // since Role_Method_Conflict_1::Method was aliased, this calls the AliasedMethod
      var otherRole = role.Cast<Role_Method_Conflict_2>();
      Assert.AreEqual(2, otherRole.Method());
      var aliasedRole = role.Cast<Role_Method_Conflict_Resolved_Through_Aliasing_Composition.View>();
      Assert.AreEqual(1, aliasedRole.AliasedMethod());
    }
  }

  public class Role_Method_Conflict_Exclude_Too_Much_Composition : Does<Role_Method_Conflict_1>, Does<Role_Method_Conflict_2>, Does<Role_Method_Conflict_Exclude_Too_Much_Composition.View1>, Does<Role_Method_Conflict_Exclude_Too_Much_Composition.View2> {
    interface View1 : RoleView<Role_Method_Conflict_1> { [Exclude] int Method(); }
    interface View2 : RoleView<Role_Method_Conflict_2> { [Exclude] int Method(); }
  }

  public class Role_Method_Conflict_Composition_View_With_Multiple_Roles : Does<Role_Method_Conflict_1>, Does<Role_Method_Conflict_2>, Does<Role_Method_Conflict_Composition_View_With_Multiple_Roles.View> {
    interface View : RoleView<Role_Method_Conflict_1>, RoleView<Role_Method_Conflict_2> { 
      [Exclude] int Method(); 
    }
  }

  public class Role_Method_Conflict_Alias_Both_Composition : Does<Role_Method_Conflict_1>, Does<Role_Method_Conflict_2>, Does<Role_Method_Conflict_Alias_Both_Composition.View1>, Does<Role_Method_Conflict_Alias_Both_Composition.View2> {
    public interface View1 : RoleView<Role_Method_Conflict_1> { [Aliasing("Method")] int AliasedMethod1(); }
    public interface View2 : RoleView<Role_Method_Conflict_2> { [Aliasing("Method")] int AliasedMethod2(); }
  }
  public class Role_Method_Conflict_Alias_Both_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var composition = new Role_Method_Conflict_Alias_Both_Composition();
      var role1 = composition.As<Role_Method_Conflict_1>();
      Assert.AreEqual(1, role1.Method());
      var role2 = composition.As<Role_Method_Conflict_2>();
      Assert.AreEqual(2, role2.Method());
      var view1 = composition.As<Role_Method_Conflict_Alias_Both_Composition.View1>();
      Assert.AreEqual(1, view1.AliasedMethod1());
      var view2 = composition.As<Role_Method_Conflict_Alias_Both_Composition.View2>();
      Assert.AreEqual(2, view2.AliasedMethod2());

      var compositionType = typeof(Role_Method_Conflict_Alias_Both_Composition);
      ReflectionAssert.HasntMethod("Method", compositionType);
      ReflectionAssert.HasMethod("AliasedMethod1", compositionType);
      ReflectionAssert.HasMethod("AliasedMethod2", compositionType);

      ReflectionAssert.HasInterfaceMap(
        new Dictionary<string, string> { { "Method", "AliasedMethod1" } },
        compositionType, typeof(Role_Method_Conflict_1));
      ReflectionAssert.HasInterfaceMap(
        new Dictionary<string, string> { { "Method", "AliasedMethod2" } },
        compositionType, typeof(Role_Method_Conflict_2));
    }
  }

  public class Role_Method_Alias_Conflict_Composition : Does<Role_Method_Conflict_1>, Does<Role_Method_Conflict_2>, Does<Role_Method_Alias_Conflict_Composition.View1>, Does<Role_Method_Alias_Conflict_Composition.View2> {
    // the aliased methods are still in conflict
    // TODO: create a better test case without error MethodsWithConflictingSignatures (only Conflict)
    interface View1 : RoleView<Role_Method_Conflict_1> { [Aliasing("Method")] int AliasedMethod(); }
    interface View2 : RoleView<Role_Method_Conflict_2> { [Aliasing("Method")] int AliasedMethod(); }
  }

  // TODO: role view as a class and struct; role view being implemented by a class; generic role view; role view that implements other interfaces than RoleView`1
  [RoleTest(
    CompositionType = typeof(Role_For_Role_View_With_Class_Composition),
    ExpectedCompositionError = Error.Code.RoleViewIsNotAnInterface)]
  [RoleTest(
    CompositionType = typeof(Role_For_Role_View_With_Interface_Composition))]
  public class Role_For_Role_View : Role {
    public void Method() { }
  }
  public class Role_For_Role_View_With_Class_Composition :
    Does<Role_For_Role_View>, Does<Role_For_Role_View_With_Class_Composition.View_As_Class> {
    public class View_As_Class : RoleView<Role_For_Role_View> { 
      [Aliasing("Method")] void AliasedMethod() {}
    }
  }
  public class Role_For_Role_View_With_Interface_Composition :
    Does<Role_For_Role_View>, Does<Role_For_Role_View_With_Interface_Composition.View_As_Interface> {
    public interface View_As_Interface : RoleView<Role_For_Role_View> {
      [Aliasing("Method")] void AliasedMethod();
    }
  }

  [RoleTest(
    CompositionType = typeof(Role_With_Method_To_Be_Aliased_Composition))]
  public class Role_With_Method_To_Be_Aliased : Role {
    public int Method() { return 42; }
  }
  public class Role_With_Method_To_Be_Aliased_Composition : Does<Role_With_Method_To_Be_Aliased>, Does<Role_With_Method_To_Be_Aliased_Composition.View> {
    interface View : RoleView<Role_With_Method_To_Be_Aliased> {
      [Aliasing("Method")]
      int AliasedMethod();
    }
  }
  // TODO: create test class

  [RoleTest(
    CompositionType = typeof(Generic_Role_With_Method_To_Be_Aliased_Generic_Composition<>))]
  [RoleTest(
    CompositionType = typeof(Generic_Role_With_Method_To_Be_Aliased_Composition))]
  public class Generic_Role_With_Method_To_Be_Aliased<T> : Role {
    public T Method() { return default(T); }
  }
  public class Generic_Role_With_Method_To_Be_Aliased_Generic_Composition<T> : Does<Generic_Role_With_Method_To_Be_Aliased<T>>, Does<Generic_Role_With_Method_To_Be_Aliased_Generic_Composition<T>.View> {
    interface View : RoleView<Generic_Role_With_Method_To_Be_Aliased<T>> {
      [Aliasing("Method")]
      T AliasedMethod();
    }
  }
  // TODO: create test class
  public class Generic_Role_With_Method_To_Be_Aliased_Composition : Does<Generic_Role_With_Method_To_Be_Aliased<string>>, Does<Generic_Role_With_Method_To_Be_Aliased_Composition.View> {
    interface View : RoleView<Generic_Role_With_Method_To_Be_Aliased<string>> {
      [Aliasing("Method")]
      string AliasedMethod();
    }
  }
  // TODO: create test class

  [RoleTest(CompositionType = typeof(Generic_Role_With_Non_Generic_Method_Generic_Composition<>))]
  [RoleTest(CompositionType = typeof(Generic_Role_With_Non_Generic_Method_Composition))]
  [RoleTest(CompositionType = typeof(Generic_Role_With_Non_Generic_Method_Generic_Aliased_Composition<>))]
  [RoleTest(CompositionType = typeof(Generic_Role_With_Non_Generic_Method_Aliased_Composition))]
  public class Generic_Role_With_Non_Generic_Method<T> : Role {
    // TODO: add non-generics to all generic examples!
    public int Method() { return 42; } // there's nothing generic about this method, but it's part of a generic role
  }
  public class Generic_Role_With_Non_Generic_Method_Generic_Composition<T> : Does<Generic_Role_With_Non_Generic_Method<T>> { }
  public class Generic_Role_With_Non_Generic_Method_Composition : Does<Generic_Role_With_Non_Generic_Method<string>> { }
  public class Generic_Role_With_Non_Generic_Method_Generic_Aliased_Composition<T> : Does<Generic_Role_With_Non_Generic_Method<T>>, Does<Generic_Role_With_Non_Generic_Method_Generic_Aliased_Composition<T>.View> {
    interface View : RoleView<Generic_Role_With_Non_Generic_Method<T>> {
      [Aliasing("Method")]
      int AliasedMethod();
    }
  }
  public class Generic_Role_With_Non_Generic_Method_Aliased_Composition : Does<Generic_Role_With_Non_Generic_Method<string>>, Does<Generic_Role_With_Non_Generic_Method_Aliased_Composition.View> {
    interface View : RoleView<Generic_Role_With_Non_Generic_Method<string>> {
      [Aliasing("Method")]
      int AliasedMethod();
    }
  }

  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_For_Return_Value_Conflict_2) },
    CompositionType = typeof(Role_For_Return_Value_Conflict_Composition),
    ExpectedCompositionError = Error.Code.MethodsWithConflictingSignatures)]
  public class Role_For_Return_Value_Conflict_1 : Role {
    public int Method() { return 42; }
  }
  public class Role_For_Return_Value_Conflict_2 : Role {
    public string Method() { return "42"; }
  }
  public class Role_For_Return_Value_Conflict_Composition : Does<Role_For_Return_Value_Conflict_1>, Does<Role_For_Return_Value_Conflict_2> {
  }

  #endregion

  #region Switchable

  [RoleTest(
    OtherRoles = new Type[] { typeof(RDimmable) },
    CompositionType = typeof(Light),
    OtherCompositions = new Type[] { typeof(DimmableLight) },
    TestType = typeof(DimmableLightTest))]
  public class RSwitchable : Role {
    public void TurnOn() {
      _on = true;
    }
    public void TurnOff() {
      _on = false;
    }
    public bool IsOn {
      get { return _on; }
    }
    public bool IsOff {
      get { return !_on; }
    }
    bool _on = false;
  }
  public class RDimmable : Role {
    public RDimmable() {
      Intensity = Min;
    }
    public int Intensity { get; private set; }
    public void Increase(int steps) {
      Intensity = Math.Min(Max, Intensity + steps * Step);
    }
    public void Decrease(int steps) {
      Intensity = Math.Max(Min, Intensity - steps * Step);
    }
    protected int Max { get { return 100; } }
    protected int Min { get { return 10; } }
    protected int Step { get { return 10; } }
  }
  public class Light : Does<RSwitchable> { }
  public class DimmableLight : Light, Does<RDimmable> { }
  public class DimmableLightTest : DynamicTestFixture {
    public override void Test() {
      var bedroomLight = new DimmableLight();
      Assert.AreEqual(false, bedroomLight.As<RSwitchable>().IsOn);
      Assert.AreEqual(true, bedroomLight.As<RSwitchable>().IsOff);

      bedroomLight.As<RSwitchable>().TurnOn();
      Assert.AreEqual(true, bedroomLight.As<RSwitchable>().IsOn);
      Assert.AreEqual(false, bedroomLight.As<RSwitchable>().IsOff);

      Assert.AreEqual(10, bedroomLight.As<RDimmable>().Intensity); // min intensity
      bedroomLight.As<RDimmable>().Increase(5);
      Assert.AreEqual(10 + 5 * 10, bedroomLight.As<RDimmable>().Intensity); // based on default values in RDimmable
    }
  }

  #endregion

  [RoleTest(
    // TODO: the trace output is misleading!
    ExpectedRoleWarning = Warning.Code.PublicStaticMethodRelocation,
    CompositionType = typeof(Role_With_Static_Method_Composition),
    TestType = typeof(Role_With_Static_Method_Composition_Test))]
  public class Role_With_Static_Method : Role {
    public static int Method() { return 42; }
  }
  public class Role_With_Static_Method_Composition : Does<Role_With_Static_Method> { }
  public class Role_With_Static_Method_Composition_Test : DynamicTestFixture {
    public override void Test() {
      Assert.AreEqual(42, Role_With_Static_Method.Method());
      ReflectionAssert.HasntMethod("Method", typeof(Role_With_Static_Method));
      ReflectionAssert.HasntMethod("Method", typeof(Role_With_Static_Method_Composition));
    }
  }

  [RoleTest(ExpectedRoleError = Error.Code.TypeCantInheritFromRole)]
  public class Base_Role : Role { }
  public class Class_That_Inherits_From_Role : Base_Role { }

  [RoleTest(ExpectedRoleError = Error.Code.TypeCantInheritFromRole)]
  public class Base_Role_2 : Role { }
  public class Class_That_Has_A_Class_That_Inherits_From_Role {
    class Nested : Base_Role_2 { }
  }

  [RoleTest(RunGlobalChecks = true, ExpectedGlobalCheckError = Error.Code.TypeCantInheritFromRole)]
  public interface Interface_Role : Role { }
  public class Class_That_Inherits_From_Interface_Role : Interface_Role { }

  [RoleTest(RunGlobalChecks = true, ExpectedGlobalCheckError = Error.Code.TypeCantInheritFromRole)]
  public interface Interface_Role_2 : Role { }
  public class Class_That_Has_A_Class_That_Inherits_From_Interface_Role {
    class Nested : Interface_Role_2 { }
  }

  [RoleTest(RunGlobalChecks = true, ExpectedGlobalCheckError = Error.Code.TypeCantInheritFromRole)]
  public interface Interface_Role_3 : Role { }
  public interface Interface_That_Inherits_From_Interface_Role : Interface_Role_3 { }

  [RoleTest(RunGlobalChecks = true, ExpectedGlobalCheckError = Error.Code.TypeCantInheritFromRole)]
  public interface Interface_Role_4 : Role { }
  public class Class_That_Has_An_Interface_That_Inherits_From_Interface_Role {
    interface Nested : Interface_Role_4 { }
  }

  [RoleTest(ExpectedRoleError = Error.Code.RoleInstantiated)]
  public class Role_That_Gets_Instantiated : Role { }
  public class Class_That_Instantiates_Role {
    Role role = new Role_That_Gets_Instantiated();
  }

  [RoleTest(ExpectedRoleError = Error.Code.RoleComposesItself)]
  public class Recursive_Role : Role, Does<Recursive_Role> { }

  [CompositionTest(
    ExpectedCompositionError = Error.Code.CompositionWithTypeParameter)]
  public class Composition_With_Generic_Role<T> : Does<T> where T : Role { }

  [RoleTest(
    ExpectedRoleError = Error.Code.RoleHasExplicitInterfaceImplementation)] // TODO: support this scenario and remove this error
    //CompositionType = typeof(Role_With_Explicit_Interface_Implemetation_Composition),
    //TestType = typeof(Role_With_Explicit_Interface_Implemetation_Composition_Test))]
  public class Role_With_Explicit_Interface_Implemetation : IDisposable, Role {
    void IDisposable.Dispose() { }
  }
  public class Role_With_Explicit_Interface_Implemetation_Composition : Does<Role_With_Explicit_Interface_Implemetation> {
  }
  public class Role_With_Explicit_Interface_Implemetation_Composition_Test {
    public static void Test() {
      var disposable = new Role_With_Explicit_Interface_Implemetation_Composition().As<Role_With_Explicit_Interface_Implemetation>() as IDisposable;
      Assert.NotNull(disposable);
    }
  }


  // TODO:

  public class Base_Composition : Does<Empty_Role> { }
  public class Derived_Composition : Base_Composition, Does<Empty_Role> { }

  // public class Composition_Same_Role_Twice : Does<Empty_Role>, Does<Empty_Role> { }
  //    yields C# compiler error: 'Does<...>' is already listed in interface list
  //    but I should treat this if it's not a PEVerify error

  public class Cyclic_Role1 : Role, Does<Cyclic_Role2> { }
  public class Cyclic_Role2 : Role, Does<Cyclic_Role1> { }

  public class Base_Role<T> : Role { }
  public class Role_That_Uses_Base_Role_1<T> : Base_Role<T> { }
  public class Role_That_Uses_Base_Role_2<T> : Base_Role<T> { }
  public class Confusing_Composition1 : Does<Role_That_Uses_Base_Role_1<string>>, Does<Role_That_Uses_Base_Role_2<string>> { }
  public class Confusing_Composition2 : Does<Role_That_Uses_Base_Role_1<int>>, Does<Role_That_Uses_Base_Role_2<string>> { }
  public class Confusing_Composition3<T> : Does<Role_That_Uses_Base_Role_1<int>>, Does<Role_That_Uses_Base_Role_2<T>> { }
  public class Confusing_Composition4<T> : Does<Role_That_Uses_Base_Role_1<T>>, Does<Role_That_Uses_Base_Role_2<T>> { }
  public class Confusing_Composition5 : Does<Role_That_Uses_Base_Role_1<string>> { }
  public class Confusing_Composition6 : Confusing_Composition5, Does<Role_That_Uses_Base_Role_2<string>> { }
  // TODO: there're many more combinations of this....

  public class Role_With_Virtual_Method : Role {
    public virtual int Method() { return 42; } // TODO?
  }

  public class Role_With_Override_Method : Role {
    public override string ToString() { return "Test"; } // TODO?
  }

  public class Role_With_Override_Method_Calling_Base : Role {
    public override string ToString() { return "Test " + base.ToString(); } // TODO?
  }

  public class Role_With_New_Method : Role {
    public new string ToString() { return null; } // TODO?
  }

  class Role_With_Protected_New_Method : Role {
    protected new string ToString() { return null; }
  }

  class Role_With_Protected_Method : Role {
    protected void Method() { }
  }

  class Base_For_Role_With_Protected_Override_Method {
    protected virtual void Method() { }
  }
  class Role_With_Protected_Override_Method : Base_For_Role_With_Protected_Override_Method, Role {
    protected override void Method() { base.Method(); }
  }

  class Base_Class_With_Protected_Virtual_Method {
    protected virtual int Method() { return 42; }
  }
  [RoleTest(
    CompositionType = typeof(Role_With_Matching_Protected_Method_From_Base_Class_Composition))]
  class Role_With_Matching_Protected_Method_From_Base_Class : Role {
    protected int Method() { return 78; }
  }
  class Role_With_Matching_Protected_Method_From_Base_Class_Composition : Base_Class_With_Protected_Virtual_Method, Does<Role_With_Matching_Protected_Method_From_Base_Class> { }

  [RoleTest(
    Description = "Role with base method call should call corresponding composition base method.",
    CompositionType = typeof(Role_With_Base_Method_Call_Composition),
    TestType = typeof(Role_With_Base_Method_Call_Composition_Test))]
  public class Role_With_Base_Method_Call : Role {
    public override string ToString() {
      return "[Role] " + base.ToString(); // base calls are "virtual"
    }
  }
  public class Base_For_Role_With_Base_Method_Call_Composition {
    public override string ToString() {
      return "[Base]";
    }
  }
  public class Role_With_Base_Method_Call_Composition : Base_For_Role_With_Base_Method_Call_Composition, Does<Role_With_Base_Method_Call> {
  }
  public class Role_With_Base_Method_Call_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var c = new Role_With_Base_Method_Call_Composition();
      Assert.AreEqual("[Role] [Base]", c.ToString());
    }
  }

  // TODO: all of the above with conflicts! Ouch!

  #region Constructors and Destructors

  class Constructor { public static bool Created = false; }
  [RoleTest(
    CompositionType = typeof(Role_With_Public_Parameterless_Constructor_Composition),
    TestType = typeof(Role_With_Public_Parameterless_Constructor_Composition_Test))]
  class Role_With_Public_Parameterless_Constructor : Role {
    public Role_With_Public_Parameterless_Constructor() { Constructor.Created = true; }
  }
  class Role_With_Public_Parameterless_Constructor_Composition : Does<Role_With_Public_Parameterless_Constructor> { }
  class Role_With_Public_Parameterless_Constructor_Composition_Test : DynamicTestFixture {
    public override void Test() {
      Constructor.Created = false;
      new Role_With_Public_Parameterless_Constructor_Composition();
      Assert.IsTrue(Constructor.Created);
    }
  }

  [RoleTest(
    CompositionType = typeof(Abstract_Role_With_Protected_Auto_Constructor_Composition),
    TestType = typeof(Abstract_Role_With_Protected_Auto_Constructor_Composition_Test))]
  abstract class Abstract_Role_With_Protected_Auto_Constructor : Role {
    // abstract classes have an automatically implemented protected constructor
  }
  class Abstract_Role_With_Protected_Auto_Constructor_Composition : Does<Abstract_Role_With_Protected_Auto_Constructor> { }
  class Abstract_Role_With_Protected_Auto_Constructor_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Abstract_Role_With_Protected_Auto_Constructor_Composition().
        As<Abstract_Role_With_Protected_Auto_Constructor>();
      Assert.IsNotNull(role);
    }
  }

  [RoleTest(ExpectedRoleError = Error.Code.RoleCannotContainParameterizedConstructor)]
  class Role_With_Public_Parameterized_Constructor : Role {
    public Role_With_Public_Parameterized_Constructor(string name) { }
  }

  class StaticConstructor { public static bool Called = false; }
  [RoleTest(
    CompositionType = typeof(Role_With_Static_Constructor_Composition),
    TestType = typeof(Role_With_Static_Constructor_Composition_Test),
    Ignore = true)]
  class Role_With_Static_Constructor : Role {
    static Role_With_Static_Constructor() {
      StaticConstructor.Called = true;
    }
  }
  class Role_With_Static_Constructor_Composition : Does<Role_With_Static_Constructor> { }
  class Role_With_Static_Constructor_Composition_Test : DynamicTestFixture {
    public override void Test() {
      new Role_With_Static_Constructor_Composition();
      Assert.IsTrue(StaticConstructor.Called, "Should have called the static constructor after creation");
    }
  }

  class Destructor { public static bool Called = false; }
  [RoleTest(
    CompositionType = typeof(Role_With_Destructor_Composition),
    TestType = typeof(Role_With_Destructor_Composition_Test))]
  class Role_With_Destructor : Role {
    // TODO: create a test with a destructor that calls methods on the class itself
    ~Role_With_Destructor() { Destructor.Called = true; }
  }
  class Role_With_Destructor_Composition : Does<Role_With_Destructor> {
  }
  class Role_With_Destructor_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var composition = new Role_With_Destructor_Composition();
      Assert.IsFalse(Destructor.Called);
      composition = null;
      GC.Collect();
      GC.WaitForPendingFinalizers();
      Assert.IsTrue(Destructor.Called);
    }
  }

  #endregion

  #region Fields

  [RoleTest(
    ExpectedRoleWarning = Warning.Code.PublicStaticFieldRelocation,
    TestType = typeof(Role_With_Public_Static_Field_Test))]
  class Role_With_Public_Static_Field : Role {
    public static string Field;
  }
  class Role_With_Public_Static_Field_Test : DynamicTestFixture {
    public override void Test() {
      Assert.IsNull(Role_With_Public_Static_Field.Field);
      Role_With_Public_Static_Field.Field = "test";
      Assert.AreEqual("test", Role_With_Public_Static_Field.Field);

      var roleType = typeof(Role_With_Public_Static_Field);
      var dataClass = roleType.GetNestedType(NameProvider.GetStateClassName(roleType.Name));
      var staticField = dataClass.GetField("Field"); // this only returns public fields
      Assert.IsNotNull(staticField);
      Assert.IsTrue(staticField.IsPublic);
      Assert.IsTrue(staticField.IsStatic);
    }
  }

  [RoleTest(
    ExpectedRoleWarning = Warning.Code.PublicStaticFieldRelocation,
    TestType = typeof(Role_With_Public_Const_Field_Test))]
  public class Role_With_Public_Const_Field : Role {
    public const string ConstField = "Const";
  }
  public class Role_With_Public_Const_Field_Test : DynamicTestFixture {
    public override void Test() {
      Assert.AreEqual("Const", Role_With_Public_Const_Field.ConstField);

      var roleType = typeof(Role_With_Public_Const_Field);
      var dataClass = roleType.GetNestedType(NameProvider.GetStateClassName(roleType.Name));
      var constField = dataClass.GetField("ConstField"); // this only returns public fields
      Assert.IsNotNull(constField);
      Assert.IsTrue(constField.IsPublic);
      Assert.IsTrue(constField.IsStatic);
      Assert.IsNotNull(constField.GetRawConstantValue());
    }
  }

  #endregion

  [RoleTest(
    OtherRoles = new Type[] { typeof(Role_That_Uses_Generic_Role_With_7_Type_Parameters) },
    CompositionType = typeof(Generic_Role_With_7_Type_Parameters_Composition),
    OtherCompositions = new Type[] { typeof(Role_That_Uses_Generic_Role_With_7_Type_Parameters_Composition) },
    TestType = typeof(Generic_Role_With_7_Type_Parameters_Composition_Test))]
  public class Generic_Role_With_7_Type_Parameters<T, U, V, W, X, Y, Z> : Role
    where Z : EventArgs
    where T : IComparable<U>
    where W : class, new()
    where X : struct {
    public X PublicMethodExtraTypeParameters<A, B>(T p1, out U p2, ref A p3, ICollection<T> ts, params B[] p4)
    where B : struct {
      p2 = default(U);
      return default(X);
    }
    public X PublicMethod(T p1, out U p2, ref W p3, ICollection<U> us, params Y[] p4) { // no extra type parameters
      p2 = default(U);
      return default(X);
    }
    public Y PublicAutoProperty { get; set; }
    public W this[X x, Y y] { get { return new W(); } set { } } // indexer
    public delegate A PublicDelegate<A>(T p1, out U p2, ref V p3, params W[] p4);
    public class Public_Nested_Generic_Class<A, B> {
      public void PublicNestedMethod(T t, A a) { }
    }
  }
  public class Generic_Role_With_7_Type_Parameters_Composition : Does<Generic_Role_With_7_Type_Parameters<int, int, int, object, int, int, EventArgs>> { }
  public class Generic_Role_With_7_Type_Parameters_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var role = new Generic_Role_With_7_Type_Parameters_Composition().As<Generic_Role_With_7_Type_Parameters<int, int, int, object, int, int, EventArgs>>();
      int outParam = 43;
      int refParam = 42;
      int result = role.PublicMethodExtraTypeParameters<int, int>(1, out outParam, ref refParam, null);
      Assert.AreEqual(0, outParam);
      Assert.AreEqual(0, result);
      Assert.AreEqual(42, refParam);
    }
  }
  public class Role_That_Uses_Generic_Role_With_7_Type_Parameters : Role, Does<Generic_Role_With_7_Type_Parameters<int, int, int, object, int, int, EventArgs>> { }
  public class Role_That_Uses_Generic_Role_With_7_Type_Parameters_Composition : Does<Role_That_Uses_Generic_Role_With_7_Type_Parameters> { }

  [CompositionTest]
  public class Simple_Role_With_Method_Composition : Does<Simple_Role_With_Method> { }

  [CompositionTest(RunGlobalChecks = true, ExpectedGlobalCheckError = Error.Code.TypeCantInheritFromRole)]
  public class Simple_Role_With_Method_Implementation : Simple_Role_With_Method {
    public int Method() { return 22; }
    public Simple_Role_With_Method.Cheshire Smile { get; set; }
  }

  [CompositionTest(RunGlobalChecks = true, ExpectedGlobalCheckError = Error.Code.TypeCantInheritFromRole)]
  public interface Simple_Role_With_Method_Inheritance : Simple_Role_With_Method {
  }

  [CompositionTest]
  public class Simple_Role_With_Property_Composition : Does<Simple_Role_With_Property> { }

  [CompositionTest]
  public class Role_With_Many_Members_Composition : Does<Role_With_Many_Members> { }

  [RoleTest(
    CompositionType = typeof(Generic_Role_With_ByRef_Composition<>))]
  public class Generic_Role_With_ByRef<T> : Role {
    public void Method(ref T t) { }
  }
  public class Generic_Role_With_ByRef_Composition<T> : Does<Generic_Role_With_ByRef<T>> { }

  [RoleTest(
    CompositionType = typeof(Generic_Role_With_Constraint_Composition<>))]
  public class Generic_Role_With_Constraint<T> : Role where T : class, new() {
    public T Method() { return new T(); }
  }
  public class Generic_Role_With_Constraint_Composition<T> : Does<Generic_Role_With_Constraint<T>>
    where T : class, new() { }

  [RoleTest(
    CompositionType = typeof(Role_With_Generic_Method_With_Constraint_Composition))]
  public class Role_With_Generic_Method_With_Constraint : Role {
    public T Method<T>() where T : class, new() { return new T(); }
  }
  public class Role_With_Generic_Method_With_Constraint_Composition : Does<Role_With_Generic_Method_With_Constraint> { }

  #region Diamond and Related Problems

  [RoleTest(
    OtherRoles = new Type[] { typeof(Diamond_Role_A), typeof(Diamond_Role_B), typeof(Diamond_Derived_Role) },
    CompositionType = typeof(Diamond_Composition_Base),
    OtherCompositions = new Type[] { typeof(Diamond_Derived_Role_Composition), typeof(Diamond_Composition_Derived) })]
  public class Diamond_Base_Role : Role {
    public int Value = 42;
  }
  public class Diamond_Role_A : Role, Does<Diamond_Base_Role> { }
  public class Diamond_Role_B : Role, Does<Diamond_Base_Role> { }
  public class Diamond_Composition_Base : Does<Diamond_Role_A>, Does<Diamond_Role_B> { }

  public class Diamond_Derived_Role : Role, Does<Diamond_Role_A>, Does<Diamond_Role_B> { }
  public class Diamond_Derived_Role_Composition : Does<Diamond_Derived_Role> { }

  public class Diamond_Composition_Derived : Diamond_Composition_Base, Does<Diamond_Base_Role> { }

  [RoleTest(
    CompositionType = typeof(Role_To_Be_Reused_Composition),
    OtherCompositions = new Type[] { typeof(Role_To_Be_Reused_Composition_Derived) },
    TestType = typeof(Role_To_Be_Reused_Composition_Derived_Test))]
  public class Role_To_Be_Reused : Role {
    public int Value = 42;
  }
  public class Role_To_Be_Reused_Composition : Does<Role_To_Be_Reused> { }
  public class Role_To_Be_Reused_Composition_Derived : Role_To_Be_Reused_Composition, Does<Role_To_Be_Reused> { }
  public class Role_To_Be_Reused_Composition_Derived_Test : DynamicTestFixture {
    public override void Test() {
      var type = typeof(Role_To_Be_Reused_Composition_Derived);
      var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.AreEqual(0, fields.Length, "The field for the role state class should not be generated in the child class!");
    }
  }

  class Constructors {
    public static List<string> Sequence = new List<string>();
  }
  [RoleTest(
    OtherRoles = new Type[] { typeof(Derived_Role_With_Constructor_1), typeof(Derived_Role_With_Constructor_2) },
    CompositionType = typeof(Diamond_Composition),
    TestType = typeof(Diamond_Composition_Test))]
  public class Base_Role_With_Constructor : Role {
    public Base_Role_With_Constructor() {
      Constructors.Sequence.Add("Base_Role_With_Constructor");
    }
  }
  public class Derived_Role_With_Constructor_1 : Role, Does<Base_Role_With_Constructor> {
    public Derived_Role_With_Constructor_1() {
      Constructors.Sequence.Add("Derived_Role_With_Constructor_1");
    }
  }
  public class Derived_Role_With_Constructor_2 : Role, Does<Base_Role_With_Constructor> {
    public Derived_Role_With_Constructor_2() {
      Constructors.Sequence.Add("Derived_Role_With_Constructor_2");
    }
  }
  public class Diamond_Composition : Does<Derived_Role_With_Constructor_1>, Does<Derived_Role_With_Constructor_2> { }
  public class Diamond_Composition_Test : DynamicTestFixture {
    public override void Test() {
      var composition = new Diamond_Composition();

      var sequence = new List<string>(Constructors.Sequence);
      Constructors.Sequence.Clear();

      sequence.ForEach(s => Console.WriteLine("SEQ: {0}", s));

      Assert.AreEqual(3, sequence.Count);
      Assert.AreEqual("Base_Role_With_Constructor", sequence[0]);
      // the other roles follow the (ahem) declaration order [!]
      Assert.AreEqual("Derived_Role_With_Constructor_1", sequence[1]);
      Assert.AreEqual("Derived_Role_With_Constructor_2", sequence[2]);
    }
  }

  #endregion

  // "composite" tests

  class Test_Class { // NOTE: maintain in synch with Test_With_Nested_Class.TestClass
    public string PublicMethod(string p1, out int p2, ref float p3, params string[] p4) {
      p2 = 1;
      return "test";
    }
    public string PublicAutoProperty { get; set; }
    public int this[int x, int y] { get { return 0; } set { } } // indexer // TODO: check this with a backing property
    public delegate string PublicDelegate(string p1, out int p2, ref float p3, params string[] p4);
    public class Public_Nested_Class { }
  }

  class Test_With_Nested_Class {

    public class Test_Class { // NOTE: maintain in synch with TestClass
      public string PublicMethod(string p1, out int p2, ref float p3, params string[] p4) {
        p2 = 1;
        return "test";
      }
      public string PublicAutoProperty { get; set; }
      public int this[int x, int y] { get { return 0; } set { } } // indexer // TODO: check this with a backing property
      public delegate string PublicDelegate(string p1, out int p2, ref float p3, params string[] p4);
      public class Public_Nested_Class { }
    }

  }

  class Test_With_Nested_Generic_Class<I, J, K>
    where I : ICloneable, new()
    where J : struct
    where K : MarshalByRefObject, new() {

    public class Generic_Test_Class<T, U, V, W, X, Y, Z> // NOTE: maintain in synch with Generic_Test_Class
      where Z : EventArgs
      where T : IComparable<U>
      where W : class, new()
      where X : struct {
      public X PublicMethodExtraTypeParameters<A, B>(T p1, out U p2, ref A p3, ICollection<T> ts, params B[] p4) // extra type parameters
      where B : struct {
        p2 = default(U);
        return default(X);
      }
      public X PublicMethod(T p1, out U p2, ref W p3, ICollection<U> us, params Y[] p4) { // no extra type parameters
        p2 = default(U);
        return default(X);
      }
      public Y PublicAutoProperty { get; set; }
      public W this[X x, Y y] { get { return new W(); } set { } } // indexer
      public delegate A PublicDelegate<A>(T p1, out U p2, ref V p3, params W[] p4);
      public class Public_Nested_Generic_Class<A, B> {
        public void PublicNestedMethod(T t, A a) { }
      }

      // extra method (not in 
      public K Public_Method_That_Uses_Enclosing_Type_Parameters(I i) {
        i.Clone();
        var k = new K();
        k.CreateObjRef(typeof(I));
        return k;
      }
    }

  }

  // others.... TODO: remove!!

  interface Test_Interface {
    string Method(string p1, out int p2, ref float p3, params string[] p4);
    string Property { get; set; }
    int this[int x, int y] { get; set; } // indexer
    event EventHandler Event;
  }

  abstract class Abstract_Test_Class { }
  // TODO: hierarchies...

  class Weirdness {
    //[CLSCompliant(false)]
    public void VariableArguments(__arglist) { // http://msdn.microsoft.com/en-us/library/ms182366%28VS.80%29.aspx
      ArgIterator argumentIterator = new ArgIterator(__arglist);
      for (int i = 0; i < argumentIterator.GetRemainingCount(); i++) {
        Console.WriteLine( __refvalue(argumentIterator.GetNextArg(), string));
      }
    }
  }

}
