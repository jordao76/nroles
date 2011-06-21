using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.Support.PlaceholderWarnings {

  public class RDeepThought : Role {
    public int Answer() { return 42; }
  }

  [CompositionTest(
    Description = "Placeholder in composition method that doesn't match role method should issue a warning",
    RoleType = typeof(RDeepThought),
    ExpectedCompositionWarning = Warning.Code.PlaceholderDoesntMatchAnyRoleMembers)]
  public class DeepThought : Does<RDeepThought> {
    [Placeholder] public int Question() { return 4 + 2; }
  }

  [CompositionTest(
    Description = "Placeholder in composition property that doesn't match role property should issue a warning",
    RoleType = typeof(RDeepThought),
    ExpectedCompositionWarning = Warning.Code.PlaceholderDoesntMatchAnyRoleMembers)]
  public class DeeperThought : Does<RDeepThought> {
    [Placeholder] public int Question { get { return 4 + 2; } }
  }

  [CompositionTest(
    Description = "Placeholder in composition event that doesn't match role event should issue a warning",
    RoleType = typeof(RDeepThought),
    ExpectedCompositionWarning = Warning.Code.PlaceholderDoesntMatchAnyRoleMembers)]
  public class DeepestThought : Does<RDeepThought> {
    [Placeholder] public event EventHandler Question { add { } remove { } }
  }

}
