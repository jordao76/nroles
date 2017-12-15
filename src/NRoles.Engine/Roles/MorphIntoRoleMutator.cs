using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NRoles.Engine {
  
  public sealed class MorphIntoRoleMutator : IMutator {

    public IOperationResult Morph(MutationParameters parameters) {
      if (parameters.SourceType != null) return MorphType(parameters);

      parameters.Validate();
      var assembly = parameters.Assembly;
      var result = new CompositeOperationResult();
      assembly.MainModule.GetAllTypes().
        Where(type => type.IsRole() && !type.IsRoleView()).
        ForEach(role => {
          var singleResult = MorphType(
            new MutationParameters { 
              SourceType = role,
              Context = parameters.Context
            }
          );
          result.AddResult(singleResult);
        });
      return result;
    }

    MutationParameters _parameters;

    public MorphIntoRoleResult MorphType(MutationParameters parameters) {
      parameters.Validate();
      var roleType = parameters.SourceType;
      if (roleType == null) throw new ArgumentException("parameters must include a SourceType", "parameters");

      _parameters = parameters;

      if (!roleType.IsRole()) {
        throw new InvalidOperationException("Cannot morph class into role");
      }

      Tracer.TraceVerbose("Morph role: {0}", roleType);

      var result = new MorphIntoRoleResult();

      CheckRole(roleType, result);
      if (!result.Success) { return result; }

      PreProcessRole(roleType); // TODO: result?
      if (!result.Success) { return result; }

      var roleStateClass = ExtractRoleStateClass(roleType, result);
      if (!result.Success) { return result; }

      // TODO: Cecil bug! We need to touch the SemanticsAttributes in order for it to remain on the method!
      roleType.Methods.Select(m => m.SemanticsAttributes).ToList();

      var roleCodeClass = ExtractRoleCodeClass(roleType, result);
      if (!result.Success) { return result; }

      MorphRoleIntoInterface(roleType, result);
      if (!result.Success) { return result; }

      result.CodeType = roleCodeClass;
      result.StateType = roleStateClass;

      return result;
    }

    private void CheckRole(TypeDefinition roleType, MorphIntoRoleResult result) {
      var validator = new RoleConstraintsValidator(roleType);
      var checkResult = validator.Check(_parameters.Context);
      result.AddResult(checkResult);
    }

    private void PreProcessRole(TypeDefinition roleType) {
      var fieldToPropertyMutator = new FieldToPropertyMutator(roleType);
      fieldToPropertyMutator.Process(_parameters.Context);

      var baseClassCallsMutator = new BaseClassCallsMutator(roleType);
      baseClassCallsMutator.Process(_parameters.Context);
    }

    private void MorphRoleIntoInterface(TypeDefinition roleType, MorphIntoRoleResult result) {
      var morpher = new MorphIntoInterfaceMutator();
      var morphIntoInterfaceResult = morpher.Morph(
        new MutationParameters {
          Context = _parameters.Context,
          SourceType = roleType,
        });
      result.AddResult(morphIntoInterfaceResult);
    }

    private TypeDefinition ExtractRoleStateClass(TypeDefinition roleType, MorphIntoRoleResult result) {
      var stateClassExtractor = new ExtractStateClassMutator();
      var extractStateClassResult = stateClassExtractor.ExtractType(
        new ExtractTypeParameters {
          Context = _parameters.Context,
          SourceType = roleType,
          TargetTypeName = DetermineStateClassName(roleType.Name),
          AddTypeStrategy = AddTypeStrategy.AddAsNested
        });
      result.AddResult(extractStateClassResult);
      return extractStateClassResult.TargetType;
    }

    private TypeDefinition ExtractRoleCodeClass(TypeDefinition roleType, MorphIntoRoleResult result) {
      var staticClassExtractor = new ExtractCodeClassMutator();
      var extractStaticClassResult = staticClassExtractor.ExtractType(
        new ExtractTypeParameters {
          Context = _parameters.Context,
          SourceType = roleType,
          TargetTypeName = DetermineCodeClassName(roleType.Name),
          AddTypeStrategy = AddTypeStrategy.AddAsNested
        });
      result.AddResult(extractStaticClassResult);
      return extractStaticClassResult.TargetType;
    }

    private string DetermineCodeClassName(string roleName) {
      return NameProvider.GetCodeClassName(roleName); // TODO: look for name clashes?
    }
    private string DetermineStateClassName(string roleName) {
      return NameProvider.GetStateClassName(roleName); // TODO: look for name clashes?
    }

    IOperationResult IMutator.Mutate(MutationParameters parameters) {
      return Morph(parameters);
    }
  }

  public class MorphIntoRoleResult : CompositeOperationResult {
    public TypeDefinition CodeType { get; set; }
    public TypeDefinition StateType { get; set; }
  }

}
