using System;

namespace NRoles.Engine.Test {

  public abstract class MutationTestAttribute : Attribute {
    public Type CompositionType { get; set; }
    public Type[] OtherCompositions { get; set; }
    public Type TestType { get; set; }
    public Error.Code ExpectedGlobalCheckError { get; set; }
    public Error.Code ExpectedCompositionError { get; set; }
    public Warning.Code ExpectedCompositionWarning { get; set; }
    public Type ExpectedException { get; set; }
    public bool Ignore { get; set; }
    public abstract Type AnnotatedType { get; set; }
    public bool RunGlobalChecks { get; set; }
  }

}
