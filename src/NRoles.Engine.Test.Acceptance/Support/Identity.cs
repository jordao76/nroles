using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRoles.Engine.Test.Support {

  [RoleTest(CompositionType = typeof(Identity_Composition))]
  
  [RoleTest(
    CompositionType = typeof(Identity_Composition_With_Wrong_Type_Argument),
    ExpectedCompositionError = Error.Code.SelfTypeConstraintNotSetToCompositionType)]

  public class Identity<S> : Role { // S is the type of "this", by default
    public S Self { get { return this.Cast<S>(); } }
  }

  public class Identity_Composition : Does<Identity<Identity_Composition>> {
  }

  public class Identity_Composition_With_Wrong_Type_Argument : Does<Identity<Identity_Composition>> {
  }

  [RoleTest(
    OtherRoles = new Type[] { typeof(Identity<>) },
    CompositionType = typeof(Identity_Role<>),
    OtherCompositions = new Type[] { typeof(Identity_Role_Composition<>) })]
  public class Identity_Role<T> : Does<Identity<Identity_Role<T>>>, Role { }
  public class Identity_Role_Composition<T> : Does<Identity_Role<T>> { }

  // here, the self-type parameter flows with the new role
  [RoleTest(
    OtherRoles = new Type[] { typeof(Identity<>) },
    CompositionType = typeof(Identity_Role_With_Self_Type<>),
    OtherCompositions = new Type[] { typeof(Identity_Role_With_Self_Type_Composition) })]
  public class Identity_Role_With_Self_Type<S> : Does<Identity<S>>, Role { }
  public class Identity_Role_With_Self_Type_Composition : 
    Does<Identity_Role_With_Self_Type<Identity_Role_With_Self_Type_Composition>> { }

}
