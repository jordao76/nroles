using System.Collections.Generic;
using Mono.Cecil;

namespace NRoles.Engine {
  
  public class SelfTypeExtractor {

    public const string DefaultSelfTypeParameterName = "TSelf";
    public readonly string SelfTypeParameterName;

    public SelfTypeExtractor(string selfTypeParameterName = null) {
      SelfTypeParameterName = selfTypeParameterName ?? DefaultSelfTypeParameterName;
    }

    public TypeReference RetrieveSelfType(TypeReference selfTypeHost) {
      var definition = selfTypeHost.Resolve();
      foreach (var parameter in definition.GenericParameters) {
        if (IsSelfTypeParameter(parameter)) {
          return ((GenericInstanceType)selfTypeHost).GenericArguments[parameter.Position];
        }
      }
      return null;
    }

    public bool IsSelfTypeParameter(GenericParameter parameter) {
      return parameter.Name == SelfTypeParameterName;
    }

    public IEnumerable<RoleSelfType> RetrieveRolesSelfTypes(TypeDefinition composition) {
      var roles = composition.RetrieveDirectRoles();
      foreach (var role in roles) {
        var selfType = RetrieveSelfType(role);
        if (selfType != null) {
          yield return new RoleSelfType(role, selfType);
        }
      }
    }

  }

  public class RoleSelfType {
    public readonly TypeReference Role;
    public readonly TypeReference SelfType;

    public RoleSelfType(TypeReference role, TypeReference selfType) {
      Role = role;
      SelfType = selfType;
    }
  }

}
