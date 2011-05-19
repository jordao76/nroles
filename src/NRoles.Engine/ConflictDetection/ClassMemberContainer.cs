using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {
  
  /// <summary>
  /// Used to contain a class' and its base classes' members.
  /// Will cooperate with other member containers to detect member conflicts.
  /// </summary>
  public class ClassMemberContainer {

    public ClassMemberContainer(TypeDefinition type) {
      if (type == null) throw new ArgumentNullException("type");
      Members = RetrieveMembers(type);
    }

    public IEnumerable<ClassMember> Members { get; private set; }

    // TODO: move all this static code!
    private static IEnumerable<ClassMember> RetrieveMembers(TypeDefinition type) {
      var members = new List<ClassMember>();
      var inherited = false;
      TypeReference currentType = type;
      do {
        var currentMembers = RetrieveDirectMembers(currentType, inherited);
        AddMembers(members, currentMembers);
        inherited = true;
        currentType = currentType.Resolve().BaseType;
      } while (currentType != null);
      return members;
    }

    private static IEnumerable<ClassMember> RetrieveDirectMembers(TypeReference type, bool inherited) {
      var visitor = new MemberReaderVisitor();
      type.Resolve().Accept(visitor);
      return visitor.Members.Select(definition => new ClassMember(type, definition, inherited));
    }

    private static void AddMembers(List<ClassMember> memberSink, IEnumerable<ClassMember> membersToAdd) {
      memberSink.AddRange(
        membersToAdd.
          Where(memberToAdd => // O(n^2)
            // TODO: what if the methods are "hide by name"?
            !memberSink.Any(member => MemberMatcher.IsMatch(member.ResolveContextualDefinition(), memberToAdd.ResolveContextualDefinition()))).
          Where(memberToAdd => !memberToAdd.IsInherited || IsVisibleInSubclass(memberToAdd.Definition)));
    }

    private static bool IsVisibleInSubclass(IMemberDefinition member) {
      //TODO! internal and in another assembly are also not visible!
      //  also take into account the InternalsVisibleTo attribute!
      //  and ProtectedANDInternal

      if (member == null) return false;

      var method = member as MethodDefinition;
      if (method != null) {
        return !method.IsPrivate;
      }

      var property = member as PropertyDefinition;
      if (property != null) {
        var getterIsVisible = IsVisibleInSubclass(property.GetMethod);
        var setterIsVisible = IsVisibleInSubclass(property.SetMethod);
        // TODO: others
        return getterIsVisible || setterIsVisible;
      }

      var @event = member as EventDefinition;
      if (@event != null) {
        var adderIsVisible = IsVisibleInSubclass(@event.AddMethod);
        var removerIsVisible = IsVisibleInSubclass(@event.RemoveMethod);
        var invokerIsVisible = IsVisibleInSubclass(@event.InvokeMethod);
        // TODO: others
        return adderIsVisible || removerIsVisible || invokerIsVisible;
      }

      var field = member as FieldDefinition;
      if (field != null) {
        return !field.IsPrivate;
      }

      throw new InvalidOperationException();
    }

  }

}
