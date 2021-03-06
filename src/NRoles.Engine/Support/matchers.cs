﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  public static class TypeMatcher {

    public static bool IsMatch(TypeReference a, TypeReference b) {
      if (a is TypeSpecification || b is TypeSpecification) {
        if (a.GetType() != b.GetType()) {
          return false;
        }
        return IsMatch((TypeSpecification)a, (TypeSpecification)b);
      }

      if (a is GenericParameter || b is GenericParameter) {
        if (a.GetType() != b.GetType()) {
          return false;
        }
      }

      return a.FullName == b.FullName;
    }

    static bool IsMatch(TypeSpecification a, TypeSpecification b) {
      if (a is GenericInstanceType) {
        return IsMatch((GenericInstanceType)a, (GenericInstanceType)b);
      }

      return IsMatch(a.ElementType, b.ElementType);
    }

    static bool IsMatch(GenericInstanceType a, GenericInstanceType b) {
      if (!IsMatch(a.ElementType, b.ElementType)) {
        return false;
      }

      if (a.GenericArguments.Count != b.GenericArguments.Count) {
        return false;
      }

      if (!a.HasGenericArguments) {
        return true;
      }

      for (int i = 0; i < a.GenericArguments.Count; i++) {
        if (!IsMatch(a.GenericArguments[i], b.GenericArguments[i])) {
          return false;
        }
      }

      return true;
    }

  }

  public static class MemberMatcher {

    public static bool IsMatch(IMemberDefinition member1, IMemberDefinition member2) {
      if (member1.GetType() != member2.GetType()) {
        return false;
      }
      if (member1 is MethodDefinition) { 
        return MethodMatcher.IsMatch((MethodDefinition)member1, (MethodDefinition)member2);
      }
      if (member1 is PropertyDefinition) { 
        return PropertyMatcher.IsMatch((PropertyDefinition)member1, (PropertyDefinition)member2);
      }
      if (member1 is EventDefinition) { 
        return EventMatcher.IsMatch((EventDefinition)member1, (EventDefinition)member2);
      }
      if (member1 is FieldDefinition) {
        return FieldMatcher.IsMatch((FieldDefinition)member1, (FieldDefinition)member2);
      }
      throw new InvalidOperationException();
    }

  }

  public static class MethodMatcher {

    public static bool IsSignatureMatch(MethodDefinition method1, MethodDefinition method2) {
      if (method2.Name != method1.Name) {
        return false;
      }

      if (method2.Parameters.Count != method1.Parameters.Count) {
        return false;
      }

      for (int i = 0; i < method2.Parameters.Count; ++i) {
        if (!TypeMatcher.IsMatch(method2.Parameters[i].ParameterType, method1.Parameters[i].ParameterType)) {
          return false;
        }
      }

      return true;
    }

    public static bool IsMatch(MethodDefinition method1, MethodDefinition method2) {
      if (!IsSignatureMatch(method1, method2)) {
        return false;
      }

      if (!TypeMatcher.IsMatch(method2.ReturnType, method1.ReturnType)) {
        return false;
      }

      return true;
    }

  }

  public static class PropertyMatcher {

    public static bool IsMatch(PropertyDefinition property1, PropertyDefinition property2) {
      if (property1.Name != property2.Name) {
        return false;
      }

      if (!TypeMatcher.IsMatch(property1.PropertyType, property2.PropertyType)) {
        return false;
      }

      // TODO: getter and setter comparison??

      return true;
    }

  }

  public static class EventMatcher {

    public static bool IsMatch(EventDefinition event1, EventDefinition event2) {
      if (event1.Name != event2.Name) {
        return false;
      }

      if (!TypeMatcher.IsMatch(event1.EventType, event2.EventType)) {
        return false;
      }

      // TODO: event accessors comparison??

      return true;
    }

  }

  public static class FieldMatcher {

    public static bool IsMatch(FieldDefinition field1, FieldDefinition field2) {
      if (field1.Name != field2.Name) {
        return false;
      }

      if (!TypeMatcher.IsMatch(field1.FieldType, field2.FieldType)) {
        return false;
      }

      return true;
    }

  }

  public class MemberFinder {

    public readonly TypeReference SearchType;
    private IList<IMemberDefinition> _members;
    private MemberResolver _resolver;

    public MemberFinder(TypeReference searchType) {
      if (searchType == null) throw new ArgumentNullException("searchType");
      SearchType = searchType;
      _resolver = new MemberResolver(SearchType);
      ResolveMembers();
    }

    private void ResolveMembers() {
      var visitor = new MemberReaderVisitor();
      SearchType.Resolve().Accept(visitor);
      _members = visitor.Members;
    }

    public IMemberDefinition FindMatchFor(IMemberDefinition memberToMatch, string aliasing = null) {
      if (memberToMatch == null) throw new ArgumentNullException("memberToMatch");
      aliasing = aliasing ?? memberToMatch.Name;

      var matchedMember = _members.SingleOrDefault(
        targetMember => 
          IsMatch(
            _resolver.ResolveMemberDefinition(targetMember), 
            memberToMatch, 
            aliasing)
      );
      return matchedMember;
    }

    private static bool IsMatch(IMemberDefinition targetMember, IMemberDefinition memberToMatch, string aliasing) {
      var oldName = targetMember.Name;
      if (aliasing == targetMember.Name) {
        // the memberToMatch is an alias for the targetMember
        // so match against the alias name
        targetMember.Name = memberToMatch.Name;
      }
      var isMatch = MemberMatcher.IsMatch(memberToMatch, targetMember);
      targetMember.Name = oldName;
      return isMatch;
    }

  }

  public class CorrespondingMethodFinder {

    public readonly TypeDefinition CodeClass;

    public CorrespondingMethodFinder(TypeDefinition codeClass) {
      if (codeClass == null) throw new ArgumentNullException("codeClass");
      CodeClass = codeClass;
    }

    public MethodDefinition FindMatchFor(MethodDefinition methodToMatch) {
      if (methodToMatch == null) throw new ArgumentNullException("methodToMatch");
      if (!CodeClass.HasMethods) return null;

      foreach (MethodDefinition methodInCodeClass in CodeClass.Methods) {
        if (IsMatch(methodInCodeClass, methodToMatch)) {
          return methodInCodeClass;
        }
      }

      return null;
    }

    bool IsMatch(MethodDefinition methodInCodeClass, MethodDefinition methodToMatch) {
      if (methodToMatch.Name != methodInCodeClass.Name) {
        return false;
      }

      if (!TypeMatcher.IsMatch(methodToMatch.ReturnType, methodInCodeClass.ReturnType)) {
        return false;
      }

      // the code class method has one extra parameter
      if (methodToMatch.Parameters.Count + 1 != methodInCodeClass.Parameters.Count) {
        return false;
      }

      // check the first parameter in the code class
      if (!TypeMatcher.IsMatch(methodToMatch.DeclaringType, methodInCodeClass.Parameters[0].ParameterType.Resolve())) {
        return false;
      }

      for (int i = 0; i < methodToMatch.Parameters.Count; ++i) {
        if (!TypeMatcher.IsMatch(methodToMatch.Parameters[i].ParameterType, methodInCodeClass.Parameters[i + 1].ParameterType)) {
          return false;
        }
      }

      return true;
    }

  }

}
