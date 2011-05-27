using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NRoles.Engine.Test {
  
  // accesses the current assembly
  public class AssemblyAccessor {

    private static byte[] _assemblyBytes;

    private AssemblyDefinition _assembly;
    Dictionary<string, TypeDefinition> _types;

    public AssemblyAccessor() {
      if (_assemblyBytes == null) {
        LoadAssemblyBytes();
      }
      LoadAssembly();
      LoadTypes();
    }

    private void LoadAssemblyBytes() {
      var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      _assemblyBytes = File.ReadAllBytes(assemblyLocation);
    }

    private void LoadAssembly() {
      using (var stream = new MemoryStream(_assemblyBytes)) {
        _assembly = AssemblyDefinition.ReadAssembly(stream);
      }
    }

    private void LoadTypes() {
      _types = new Dictionary<string, TypeDefinition>();
      foreach (var type in _assembly.MainModule.GetAllTypes()) {
        _types.Add(type.FullName, type);
      }
    }

    public static implicit operator AssemblyDefinition(AssemblyAccessor accessor) {
      return accessor._assembly;
    }

    public TypeDefinition GetType<T>() {
      return _types[typeof(T).FullName.Replace('+', '/')];
    }

    public TypeDefinition GetType(Type type) {
      return _types[type.FullName.Replace('+', '/')];
    }

  }

}
