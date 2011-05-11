using System;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test.Composition.SelfType {

  [TestFixture]
  public class Retrieve_Single_Self_Type_Fixture : AssemblyReadonlyFixture {

    private TypeReference GetSelfTypeFromBase<TSource>() {
      return GetSelfTypeFromBase(typeof(TSource));
    }

    private TypeReference GetSelfTypeFromBase(Type sourceType) {
      var extractor = new SelfTypeExtractor();
      var source = GetType(sourceType);
      var host = source.BaseType;
      return extractor.RetrieveSelfType(host);
    }

    [Test]
    public void Test_Self_Type_Should_Be_Null_For_Non_Generic_Host() {
      Assert.IsNull(GetSelfTypeFromBase<NoSelfType>());
    }
    class NoSelfType : object { }

    class DeclaresSelfType<TSelf> { }

    [Test]
    public void Test_Self_Type_Equals_Source_Type() {
      var source = GetType<UsesSelfType>();
      var selfType = GetSelfTypeFromBase<UsesSelfType>();
      Assert.AreEqual(source.ToString(), selfType.ToString());
    }
    class UsesSelfType : DeclaresSelfType<UsesSelfType> { }

    [Test]
    public void Test_Self_Type_Equals_Generic_Source_Type() {
      var source = GetType(typeof(UsesSelfType<>));
      var selfType = GetSelfTypeFromBase(typeof(UsesSelfType<>));
      Assert.AreEqual(source.ToString(), selfType.Resolve().ToString());
      Assert.AreEqual(source.GenericParameters[0].Name, ((GenericInstanceType)selfType).GenericArguments[0].Name);
    }
    class UsesSelfType<T> : DeclaresSelfType<UsesSelfType<T>> { }

    [Test]
    public void Test_Self_Type_With_Type_Argument_Differs_From_Generic_Source_Type() {
      var source = GetType(typeof(UsesSelfTypeWrongArgument<>));
      var selfType = GetSelfTypeFromBase(typeof(UsesSelfTypeWrongArgument<>));
      Assert.AreEqual(source.ToString(), selfType.Resolve().ToString());
      Assert.AreNotEqual(source.GenericParameters[0].Name, ((GenericInstanceType)selfType).GenericArguments[0].Name);
    }
    class UsesSelfTypeWrongArgument<T> : DeclaresSelfType<UsesSelfTypeWrongArgument<string>> { }

  }

}
