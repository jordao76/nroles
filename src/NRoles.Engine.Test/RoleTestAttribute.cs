using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test {

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
  public class RoleTestAttribute : MutationTestAttribute {
    public Type RoleType { get; set; }
    public Type[] OtherRoles { get; set; }
    public Error.Code ExpectedRoleError { get; set; }
    public Warning.Code ExpectedRoleWarning { get; set; }
    public override Type AnnotatedType {
      get { return RoleType; }
      set { RoleType = value; } 
    }
    public override string ToString() {
      var sb = new StringBuilder();
      sb.Append(RoleType.Name);
      if (CompositionType != null) {
        sb.AppendFormat(" - {0}", CompositionType.Name);
      }
      if (TestType != null) {
        sb.AppendFormat(" - {0}", TestType.Name);
      }
      return sb.ToString();
    }
  }

}
