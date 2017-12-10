using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRoles.Engine.Test.ConflictDetection.ClassMemberTests {
  
  [TestFixture]
  public class ClassMemberFixture : AssemblyReadonlyFixture {

    // NOTE: just ideas...

    // ClassMember dependencies
    //   IsPlaceholder
    //   Container (~IConflictClassifier)
    //     ResolveGroup returns MemberGroup 
    //   MemberGroup (~IConflictGroup, none of the following methods are in IConflictGroup)
    //     MarkAsSuperceded <= this is just like saying: "I win the conflict!" => ResolveConflictWith(this)
    //       The opposite is to lose the conflict (excluded(overridden), placeholder(~overridden), aliased?(not overridden)) => LoseConflict(this)?
    //     ReuseSlot <= this term is only meaninful when composing
    //     Process just to find out if DoesNotImplementAbstractRoleMember

    //  MemberGroup (new interface -> IConflictResolver)
    //    ResolveConflictWith (~MarkAsSuperceded)

    public class ConflictResolutionResult : CompositeOperationResult {
      public bool HasConflict { get { return !Success; } }
      public RoleCompositionMember ResolvedMember { get; set; }
    }

    public interface IConflictResolver {
      ConflictResolutionResult ResolveConflict(IConflictGroup group);
    }

    [Test, Ignore("wip")]
    public void Non_Inherited_Should_Mark_Group_As_Superceded() {
      // Non_Inherited_Should_Win_Conflict
      var member = GetMethodByName(typeof(Simple), "Method");

      IConflictGroup group = new ContributedConflictGroup(); // TODO: change!
      group.AddMember(member);
      // TODO: add more to create a conflict?

      IConflictResolver resolver = null; // TODO: new CompositionMemberResolver();

      var result = resolver.ResolveConflict(group);

      // assert that the conflict was resolved with the member
      Assert.AreEqual(member, result.ResolvedMember);
      
      // TODO:OBSOLETE: member.Process();
    }
    class Simple {
      public void Method() { }
    }

  }

}
