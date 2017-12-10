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
      _assembly = new AssemblyAccessor(Assembly.GetExecutingAssembly().Location);
    }

    [Test, TestCaseSource(typeof(MutationTestCaseFactory<RoleTestAttribute>), "LoadTestCases")]
    public void Test_Role_Mutation(MutationTestAttribute testParameters) {
      TestMutation(testParameters);
    }


    [Test, TestCaseSource(typeof(MutationTestCaseFactory<CompositionTestAttribute>), "LoadTestCases")]
    public void Test_Composition_Mutation(CompositionTestAttribute testParameters) {
      TestMutation(testParameters);
    }

    private void TestMutation(MutationTestAttribute testParameters) {
      Run_Global_Checks(testParameters);

      Mutate_Into_Role(testParameters, testParameters.RoleType);
      if (testParameters.OtherRoles != null) {
        testParameters.OtherRoles.ForEach(role => Mutate_Into_Role(null, role));
      }

      Compose_Role(testParameters);

      if (!ErrorExpected(testParameters)) {
        VerifyAndTestAssembly(testParameters);
      }
    }

    private bool ErrorExpected(MutationTestAttribute testParameters) {
      return
        testParameters.ExpectedGlobalCheckError != 0 ||
        testParameters.ExpectedRoleError != 0 ||
        testParameters.ExpectedCompositionError != 0;
    }

    private void VerifyAndTestAssembly(MutationTestAttribute testParameters) {
      var executingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      using (var assemblyFile = new TemporaryFile(executingDir)) {
        ((AssemblyDefinition)_assembly).Write(assemblyFile.FilePath);
        Assert_Assembly(assemblyFile.FilePath);
        Test_Role(testParameters, assemblyFile.FilePath);
      }
    }

    private void Mutate_Into_Role(MutationTestAttribute testParameters, Type roleType) {
      if (roleType == null) return;
      var mutator = new MorphIntoRoleMutator();
      var runner = new MutationRunner(_assembly.GetType(roleType));
      var result = runner.Run(mutator);

      Assert_Mutate_Into_Role_Result(testParameters, result);
    }

    private static void Assert_Mutate_Into_Role_Result(MutationTestAttribute testParameters, IOperationResult result) {
      var expectedError = testParameters != null && testParameters.ExpectedRoleError != 0;
      var expectedWarning = testParameters != null && testParameters.ExpectedRoleWarning != 0;
      if (expectedError) {
        result.AssertError(testParameters.ExpectedRoleError);
      }
      if (expectedWarning) {
        result.AssertWarning(testParameters.ExpectedRoleWarning);
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
      var runner = new MutationRunner(_assembly.GetType(compositionType));
      return runner.Run(new RoleComposerMutator());
    }

    private static void Assert_Compose_Role_Result(MutationTestAttribute testParameters, IOperationResult result) {
      var expectedError = testParameters.ExpectedCompositionError != 0;
      var expectedWarning = testParameters.ExpectedCompositionWarning != 0;
      if (expectedError) {
        result.AssertError(testParameters.ExpectedCompositionError);
      }
      if (expectedWarning) {
        result.AssertWarning(testParameters.ExpectedCompositionWarning);
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
      AssemblyAssert.Verify(assemblyPath);
    }

  }

  class MutationTestCaseFactory<T> where T : MutationTestAttribute {
    public static IEnumerable<TestCaseData> LoadTestCases() {
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
        let tcd = new TestCaseData(a)
        select a.Ignore ? tcd.SetName(name).Ignore("wip") : tcd.SetName(name);
    }
  }

}
