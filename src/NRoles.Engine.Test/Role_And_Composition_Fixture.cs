using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace NRoles.Engine.Test {

  using Support;

  [TestFixture]
  public class Role_And_Composition_Fixture {

    AssemblyAccessor _assembly;

    [SetUp]
    public void SetUp() {
      _assembly = new AssemblyAccessor();
    }

    [Test, TestCaseSource(typeof(MutationTestCaseFactory<RoleTestAttribute>), "LoadTestCases")]
    public void Test_Role_Mutation(RoleTestAttribute testParameters) {
      Run_Global_Checks(testParameters);
      Mutate_Into_Role(testParameters, testParameters.RoleType);
      if (testParameters.OtherRoles != null) {
        testParameters.OtherRoles.ForEach(role => Mutate_Into_Role(null, role));
      }
      Compose_Role(testParameters);
      var noErrorExpected =
        testParameters.ExpectedGlobalCheckError == 0 &&
        testParameters.ExpectedRoleError == 0 && 
        testParameters.ExpectedCompositionError == 0;

      if (noErrorExpected) {
        VerifyAndTestAssembly(testParameters);
      }
    }

    [Test, TestCaseSource(typeof(MutationTestCaseFactory<CompositionTestAttribute>), "LoadTestCases")]
    public void Test_Composition_Mutation(CompositionTestAttribute testParameters) {
      Run_Global_Checks(testParameters);
      Compose_Role(testParameters);
      var noErrorExpected =
        testParameters.ExpectedGlobalCheckError == 0 &&
        testParameters.ExpectedCompositionError == 0;

      if (noErrorExpected) {
        VerifyAndTestAssembly(testParameters);
      }
    }

    private void VerifyAndTestAssembly(MutationTestAttribute testParameters) {
      using (var assemblyFile = new TemporaryFile(Directory.GetCurrentDirectory())) {
        ((AssemblyDefinition)_assembly).Write(assemblyFile.FilePath);
        Assert_Assembly(assemblyFile.FilePath);
        Test_Role(testParameters, assemblyFile.FilePath);
      }
    }

    private void Mutate_Into_Role(RoleTestAttribute testParameters, Type sourceType) {
      var mutator = new MorphIntoRoleMutator();
      var runner = new MutationRunner(_assembly.GetType(sourceType));
      var result = runner.Run(mutator);

      Assert_Mutate_Into_Role_Result(testParameters, result);
    }

    private static void Assert_Mutate_Into_Role_Result(RoleTestAttribute testParameters, IOperationResult result) {
      result.Messages.ForEach(m => Console.WriteLine(m));
      var expectedError = testParameters != null && testParameters.ExpectedRoleError != 0;
      var expectedWarning = testParameters != null && testParameters.ExpectedRoleWarning != 0;
      if (expectedError) {
        Assert.That(result.Messages.Any(m => m.Number == (int)testParameters.ExpectedRoleError));
      }
      if (expectedWarning) {
        Assert.That(result.Messages.Any(m => m.Number == (int)testParameters.ExpectedRoleWarning));
      }
      Assert.AreEqual(!expectedError, result.Success);
    }

    private void Compose_Role(MutationTestAttribute testParameters) {
      var compositionType = testParameters.CompositionType;
      if (compositionType == null) return;

      var result = Compose_Role(compositionType);
      Assert_Compose_Role_Result(testParameters, result);

      if (testParameters.OtherCompositions != null) {
        testParameters.OtherCompositions.ForEach(composition => Compose_Role(composition));
      }
    }

    private IOperationResult Compose_Role(Type compositionType) {
      if (compositionType == null) return null;
      var composer = new RoleComposerMutator();
      var runner = new MutationRunner(_assembly.GetType(compositionType));
      return runner.Run(new RoleComposerMutator());
    }

    private static void Assert_Compose_Role_Result(MutationTestAttribute testParameters, IOperationResult result) {
      result.Messages.ForEach(m => Console.WriteLine(m));
      var expectedError = testParameters.ExpectedCompositionError != 0;
      var expectedWarning = testParameters.ExpectedCompositionWarning != 0;
      if (expectedError) {
        Assert.That(result.Messages.Any(m => m.Number == (int)testParameters.ExpectedCompositionError));
      }
      if (expectedWarning) {
        Assert.That(result.Messages.Any(m => m.Number == (int)testParameters.ExpectedCompositionWarning));
      }
      Assert.AreEqual(!expectedError, result.Success);
    }

    private void Run_Global_Checks(MutationTestAttribute testParameters) {
      if (!testParameters.RunGlobalChecks) return;
      var mutator = new GlobalRoleChecks();
      var runner = new MutationRunner(_assembly);
      var result = runner.Run(mutator);

      Assert_Global_Checks_Result(testParameters, result);
    }

    private void Assert_Global_Checks_Result(MutationTestAttribute testParameters, IOperationResult result) {
      result.Messages.ForEach(m => Console.WriteLine(m));
      var expectedError = testParameters.ExpectedGlobalCheckError != 0;
      if (expectedError) {
        Assert.That(result.Messages.Any(m => m.Number == (int)testParameters.ExpectedGlobalCheckError));
      }
      Assert.AreEqual(!expectedError, result.Success);
    }

    private void Test_Role(MutationTestAttribute testParameters, string assemblyPath) {
      var testType = testParameters.TestType;
      if (testType == null) return;

      var domain = AppDomain.CreateDomain("DynamicTestDomain");
      try {
        var instance = (DynamicTestFixture)domain.CreateInstanceFromAndUnwrap(assemblyPath, testType.FullName);
        instance.Test();
      }
      finally {
        AppDomain.Unload(domain);
      }
    }

    private void Assert_Assembly(string assemblyPath) {
      // debug!!
      //((AssemblyDefinition)_assembly).Write(@"C:\Temp\NRoles.mutated.dll");
      //Console.WriteLine(@"SAVED ASSEMBLY: C:\Temp\NRoles.mutated.dll");

      AssemblyAssert.Verify(assemblyPath);
    }

  }

  class MutationTestCaseFactory<T> where T : MutationTestAttribute {
    public IEnumerable<TestCaseData> LoadTestCases() {
      var assembly = Assembly.GetExecutingAssembly();

      var markedTypes = from t in assembly.GetTypes()
                        where t.IsDefined(typeof(T), false)
                        select t;

      var attributes = markedTypes.
        SelectMany(mt => {
          var attrs = mt.GetCustomAttributes(typeof(T), false).Cast<T>().ToList();
          attrs.ForEach(a => a.AnnotatedType = mt);
          return attrs;
        });

      return
        from a in attributes
        let t = a.AnnotatedType
        let name = a.ToString()
        // TODO: these last 2 lines are ugly!
        let tcd = (a.ExpectedException == null ? new TestCaseData(a) : new TestCaseData(a).Throws(a.ExpectedException))
        select a.Ignore ? tcd.SetName(name).Ignore() : tcd.SetName(name);
    }
  }

}
