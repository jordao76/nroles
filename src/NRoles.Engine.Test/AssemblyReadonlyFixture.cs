using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NUnit.Framework;

namespace NRoles.Engine.Test {

  public abstract class AssemblyReadonlyFixture {
    static AssemblyAccessor _assembly = new AssemblyAccessor();
    static AssemblyReadonlyFixture() {
      new MutationContext(((AssemblyDefinition)_assembly).MainModule);
    }
    protected TypeDefinition GetType<T>() {
      return _assembly.GetType<T>();
    }
    protected TypeDefinition GetType(Type type) {
      return _assembly.GetType(type);
    }
    protected ClassMember GetMethodByName(Type t, string methodName) {
      var type = GetType(t);
      var currentType = type;
      TypeReference typeContext = type;
      MethodDefinition method = null;
      while (currentType != null &&
        (method = currentType.Methods.SingleOrDefault(m => m.Name == methodName)) == null) {
        typeContext = currentType.BaseType;
        currentType = typeContext?.Resolve();
      }
      Assert.IsNotNull(method);
      return new ClassMember(typeContext, method, typeContext != type);
    }

  }

}
