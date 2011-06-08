using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace NRoles.Engine {

  /// <summary>
  /// Some code visitors are only valid for certain types,
  /// others are valid for all types;
  /// this class scopes them appropriately.
  /// </summary>
  public class ScopingCodeVisitor : CompositeCodeVisitorBase, ICodeVisitorRegistry {
    public static readonly object AssemblyKey = new object();
    Dictionary<object, CompositeCodeVisitor> _visitors = new Dictionary<object, CompositeCodeVisitor>();
    CompositeCodeVisitor _currentVisitor;

    #region Registry

    public override void AddCodeVisitor(CodeVisitorBase visitor) {
      if (visitor == null) throw new ArgumentNullException("visitor");
      AddCodeVisitor(visitor, AssemblyKey); // AssemblyKey indicates that the visitor is valid for all types
    }
    public void AddCodeVisitor(CodeVisitorBase visitor, object key) {
      if (visitor == null) throw new ArgumentNullException("visitor");
      if (key == null) throw new ArgumentNullException("key");
      CompositeCodeVisitor visitorsForType;
      _visitors.TryGetValue(key, out visitorsForType);
      if (visitorsForType == null) {
        visitorsForType = new CompositeCodeVisitor();
        _visitors.Add(key, visitorsForType);
      }
      visitorsForType.AddCodeVisitor(visitor);
    }

    void ICodeVisitorRegistry.Register(CodeVisitorBase visitor) {
      AddCodeVisitor(visitor);
    }
    void ICodeVisitorRegistry.Register(CodeVisitorBase visitor, TypeDefinition scopedAtType) {
      AddCodeVisitor(visitor, scopedAtType);
    }

    #endregion

    #region Visiting

    public void Visit(AssemblyDefinition assembly) {
      if (assembly == null) throw new ArgumentNullException("assembly");
      foreach (var type in assembly.MainModule.GetAllTypes()) {
        BeginVisitingType(type);
        Visit(type);
        EndVisitingType();
      }
    }

    public void Visit(TypeDefinition type) {
      if (type == null) throw new ArgumentNullException("type");
      VisitMethods(type.Methods);
    }

    private void VisitMethods(IEnumerable<MethodDefinition> methods) {
      foreach (MethodDefinition method in methods) {
        var body = method.GetBody();
        if (body != null) {
          body.Accept(this);
        }
      }
    }

    public void BeginVisitingType(TypeDefinition typeBeingVisited) {
      if (typeBeingVisited == null) throw new ArgumentNullException("typeBeingVisited");

      _currentVisitor = null;

      AddVisitorToCurrentComposite(typeBeingVisited);

      // include all enclosing types from the type being visited
      GetEnclosingTypes(typeBeingVisited).ForEach(et => AddVisitorToCurrentComposite(et));

      AddVisitorToCurrentComposite(AssemblyKey); // AssemblyKey indicates that the visitor is valid for all types
    }

    private IEnumerable<TypeDefinition> GetEnclosingTypes(TypeDefinition type) {
      var current = type;
      while (current.DeclaringType != null) {
        yield return current.DeclaringType;
        current = current.DeclaringType;
      }
    }

    private void AddVisitorToCurrentComposite(object visitorKey) {
      CompositeCodeVisitor visitor;
      _visitors.TryGetValue(visitorKey, out visitor);
      if (visitor != null) {
        if (_currentVisitor == null) _currentVisitor = new CompositeCodeVisitor();
        _currentVisitor.AddCodeVisitor(visitor);
      }
    }

    internal override void Apply(Action<CodeVisitorBase> action) {
      if (_currentVisitor != null) {
        _currentVisitor.Apply(action);
      }
    }

    public void EndVisitingType() {
      _currentVisitor = null;
    }

    #endregion

  }

}
