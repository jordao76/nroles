using System;
using System.Text;

namespace NRoles.Engine.Test {

  public abstract class MutationTestAttribute : Attribute {
    public abstract Type AnnotatedType { get; set; }
    public abstract Type SupportingType { get; set; }
    public string Description { get; set; }
    public Type RoleType { get; set; }
    public Type[] OtherRoles { get; set; }
    public Error.Code ExpectedRoleError { get; set; }
    public Warning.Code ExpectedRoleWarning { get; set; }
    public Type CompositionType { get; set; }
    public Type[] OtherCompositions { get; set; }
    public Error.Code ExpectedCompositionError { get; set; }
    public Warning.Code ExpectedCompositionWarning { get; set; }
    public Error.Code ExpectedGlobalCheckError { get; set; }
    public Type TestType { get; set; }
    public bool Ignore { get; set; }
    public bool RunGlobalChecks { get; set; }

    public override string ToString() {
      return Description ?? DefaultDescription;
    }
    private string DefaultDescription {
      get {
        var sb = new StringBuilder();
        sb.Append(AnnotatedType.Name);
        if (SupportingType != null) {
          sb.AppendFormat(" - {0}", SupportingType.Name);
        }
        if (TestType != null) {
          sb.AppendFormat(" - {0}", TestType.Name);
        }
        return sb.ToString();
      }
    }
  }

}
