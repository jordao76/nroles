using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  /// <summary>
  /// Generates a default constructor that calls the <see cref="Object"/> class' constructor.
  /// </summary>
  public class DefaultConstructorGenerator {

    private readonly ModuleDefinition _module;
    private readonly TypeDefinition _targetType;

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="targetType">The type to generate the constructor on.</param>
    /// <param name="module">The module to use. If not provided is taken from the <paramref name="targetType"/>.</param>
    public DefaultConstructorGenerator(TypeDefinition targetType, ModuleDefinition module = null) {
      if (targetType == null) throw new ArgumentNullException("targetType");
      _targetType = targetType;
      _module = module ?? targetType.Module;
      if (_module == null) throw new ArgumentException("targetType does not belong to a module. You can also use the module parameter.", "targetType");
    }

    /// <summary>
    /// Creates the constructor in the target type.
    /// </summary>
    public void CreateConstructor() {
      var ctor = new MethodDefinition(
        ".ctor",
        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
        _module.Import(typeof(void)));
      EmitConstructorCode(ctor);
      _targetType.Methods.Add(ctor);
    }

    private void EmitConstructorCode(MethodDefinition ctor) {
      // call the base class (object) constructor
      var worker = ctor.Body.GetILProcessor();
      worker.Append(worker.Create(OpCodes.Ldarg_0));
      worker.Append(worker.Create(OpCodes.Call, ResolveObjectConstructor()));
      worker.Append(worker.Create(OpCodes.Ret));
    }

    private MethodReference ResolveObjectConstructor() {
      return new MethodReference(
        ".ctor",
        _module.Import(typeof(void))) {
          DeclaringType = _module.Import(typeof(object)),
          HasThis = true,
          ExplicitThis = false,
          CallingConvention = MethodCallingConvention.Default
        };
    }

  }

}
