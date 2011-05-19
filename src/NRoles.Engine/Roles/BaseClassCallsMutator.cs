using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {
  
  class BaseClassCallsMutator {
    private TypeDefinition _type;

    public BaseClassCallsMutator(TypeDefinition type) {
      if (type == null) throw new ArgumentNullException("type");
      _type = type;
    }

    MutationContext _context;

    private List<Instruction> _callsToBaseMethods;
    private List<ClassMember> _baseMethodsSet;

    public void Process(MutationContext context) {
      if (context == null) throw new ArgumentNullException("context");
      _context = context;

      _callsToBaseMethods = new List<Instruction>();
      _baseMethodsSet = new List<ClassMember>();

      _type.Methods.Cast<MethodDefinition>().
        Where(md => !md.IsAbstract).
        ForEach(md => Process(md));
      
      var virtualBaseMethodsDeclarations = 
        ResolveVirtualBaseMethodsDeclarations(_baseMethodsSet);
      
      DeclareVirtualBaseMethods(virtualBaseMethodsDeclarations);
      RewireBaseCallInstructions();
    }

    private void Process(MethodDefinition method) {
      ResolveCallsToBaseMethod(method);
      ResolveBaseMethods();
    }

    private void RewireBaseCallInstructions() {
      foreach (var instruction in _callsToBaseMethods) {
        var oldMethod = (MethodReference)instruction.Operand;
        var newMethod = ResolveNewMethodReference(oldMethod);
        instruction.OpCode = OpCodes.Callvirt;
        instruction.Operand = newMethod;
      }
    }

    private MethodReference ResolveNewMethodReference(MethodReference oldMethod) {
      var finder = new MemberFinder(_type);
      var newMethod = (MethodDefinition)finder.FindMatchFor(
        new MemberResolver(oldMethod.DeclaringType).ResolveMethodDefinition(
          oldMethod.Resolve(),
          NameProvider.GetVirtualBaseMethodName(oldMethod.DeclaringType.Name, oldMethod.Name),
          oldMethod.Resolve().Attributes));
      if (newMethod == null) throw new InvalidOperationException();
      return newMethod;
    }

    private void DeclareVirtualBaseMethods(IEnumerable<MethodDefinition> methods) { 
      // TODO: clashes?
      foreach (var method in methods) {
        _type.Methods.Add(method);
      }
    }

    private IEnumerable<MethodDefinition> ResolveVirtualBaseMethodsDeclarations(IEnumerable<ClassMember> baseMethodsSet) {
      var definitions = new List<MethodDefinition>();
      foreach (var baseMethod in baseMethodsSet) {
        var method = (MethodDefinition)baseMethod.Definition;
        var definition = new MemberResolver(baseMethod.Class).
          ResolveMethodDefinition(
            method,
            NameProvider.GetVirtualBaseMethodName(baseMethod.Class.Name, method.Name),
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot);
        definitions.Add(definition);
      }
      return definitions;
    }

    private void ResolveBaseMethods() {
      foreach (var instruction in _callsToBaseMethods) {
        var method = (MethodReference)instruction.Operand;
        if (!HasMethod(method)) {
          var classMember = new ClassMember(method.DeclaringType, method.Resolve(), isInherited: true);
          _baseMethodsSet.Add(classMember);
        }
      }
    }

    private bool HasMethod(MethodReference method) {
      foreach (var baseMethod in _baseMethodsSet) {
        if (baseMethod.Class == method.DeclaringType && baseMethod.Definition == method.Resolve()) {
          return true;
        }
      }
      return false;
    }

    private void ResolveCallsToBaseMethod(MethodDefinition method) {
      // Note: deciding if a call is a base method call is too hard in general, 
      // it's necessary to trace the stack until an ldarg.0 is found.
      // Since Object is the only base class supported for roles at the moment, 
      // this is simpler.
      
      // Look for calls (not callvirt) to methods in Object
      foreach (Instruction instruction in method.Body.Instructions) {
        if (instruction.OpCode == OpCodes.Call) {
          var calleeReference = (MethodReference)instruction.Operand;
          var callee = calleeReference.Resolve();
          if (!callee.IsConstructor && !callee.IsStatic 
            && callee.DeclaringType == _context.Module.Import(typeof(object)).Resolve()) {
            _callsToBaseMethods.Add(instruction);
          }
        }
      }
    }

  }

}
