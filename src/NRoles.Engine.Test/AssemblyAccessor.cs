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
  
  public class AssemblyAccessor {

    private static string _assemblyLocation;
    private static byte[] _assemblyBytes;
    private AssemblyDefinition _assembly;
    private Dictionary<string, TypeDefinition> _types;

    public AssemblyAccessor(string assemblyLocation = null) {
      assemblyLocation = assemblyLocation ?? Assembly.GetExecutingAssembly().Location;
      if (_assemblyLocation != assemblyLocation) {
        _assemblyBytes = null;
        _assemblyLocation = assemblyLocation;
      }
      if (_assemblyBytes == null) {
        LoadAssemblyBytes();
      }
      LoadAssembly();
      LoadTypes();
    }

    private void LoadAssemblyBytes() {
      _assemblyBytes = File.ReadAllBytes(_assemblyLocation);
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
