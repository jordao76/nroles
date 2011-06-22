using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support.PlaceholderError {

  [RoleTest(
    Description = "Placeholder in role method should issue an error",
    ExpectedRoleError = Error.Code.RoleHasPlaceholder)]
  public class RDeepThought : Role { 
    [Placeholder] public void Think() { }
  }

  [RoleTest(
    Description = "Placeholder in role property should issue an error",
    ExpectedRoleError = Error.Code.RoleHasPlaceholder)]
  public class RDeeperThought : Role {
    [Placeholder] public int Think { get; set; }
  }

  [RoleTest(
    Description = "Placeholder in role event should issue an error",
    ExpectedRoleError = Error.Code.RoleHasPlaceholder)]
  public class RDeepestThought : Role {
    #pragma warning disable 67
    [Placeholder] public event EventHandler Think;
    #pragma warning restore 67
  }

  [RoleTest(
    Description = "Placeholder in role indexer should issue an error",
    ExpectedRoleError = Error.Code.RoleHasPlaceholder)]
  public class RDeepestestThought : Role {
    [Placeholder] public int this[int x, int y] { set { } }
  }

}
