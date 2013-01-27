using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

public class AssemblyListAssemblyResolver : IAssemblyResolver {

  private readonly Dictionary<string, AssemblyDefinition> assembliesLoaded = new Dictionary<string, AssemblyDefinition>(StringComparer.InvariantCultureIgnoreCase);
  private readonly Dictionary<string, string> assembliesByName = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
  private DefaultAssemblyResolver defaultResolver = new DefaultAssemblyResolver();

  public AssemblyListAssemblyResolver(IEnumerable<string> assemblyList) {
    foreach (var path in assemblyList) {
      assembliesByName[Path.GetFileNameWithoutExtension(path)] = path;
    }
    var dirs = assemblyList.Select(x => Path.GetDirectoryName(x)).Distinct();
    foreach (var dir in dirs) {
      defaultResolver.AddSearchDirectory(dir);
    }
  }

  public AssemblyDefinition Resolve(AssemblyNameReference assemblyNameReference) {
    return Resolve(assemblyNameReference, new ReaderParameters { AssemblyResolver = this });
  }

  public AssemblyDefinition Resolve(AssemblyNameReference assemblyNameReference, ReaderParameters parameters) {
    if (assemblyNameReference == null) throw new ArgumentNullException("assemblyNameReference");
    if (parameters == null) {
      parameters = new ReaderParameters { AssemblyResolver = this };
    }

    string path;
    if (assembliesByName.TryGetValue(assemblyNameReference.Name, out path)) {
      return ResolveAssembly(path, parameters);
    }

    return defaultResolver.Resolve(assemblyNameReference, parameters);
  }

  public AssemblyDefinition Resolve(string fullName) {
    return Resolve(fullName, new ReaderParameters { AssemblyResolver = this } );
  }

  public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters) {
    if (fullName == null) throw new ArgumentNullException("fullName");
    return Resolve(AssemblyNameReference.Parse(fullName), parameters);
  }

  private AssemblyDefinition ResolveAssembly(string path, ReaderParameters parameters) {
    if (parameters.AssemblyResolver == null) {
      parameters.AssemblyResolver = this;
    }
    AssemblyDefinition assemblyDefinition;
    if (assembliesLoaded.TryGetValue(path, out assemblyDefinition)) {
      return assemblyDefinition;
    }
    assembliesLoaded[path] = assemblyDefinition = ModuleDefinition.ReadModule(path, parameters).Assembly;
    return assemblyDefinition;
  }

}
