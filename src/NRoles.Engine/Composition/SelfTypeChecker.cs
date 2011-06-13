using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {
  
  public class SelfTypeChecker {

    private SelfTypeExtractor _extractor;

    public SelfTypeChecker(string selfTypeParameterName = null) {
      _extractor = new SelfTypeExtractor(selfTypeParameterName);
    }

    public IOperationResult CheckComposition(TypeDefinition composition) {
      var rolesAndSelfTypes = _extractor.RetrieveRolesSelfTypes(composition);
      var nonMatching = rolesAndSelfTypes.Where(rs => !Matches(rs.SelfType, composition));

      var result = new OperationResult();
      nonMatching.ForEach(rs =>
        result.AddMessage(
          Error.SelfTypeConstraintNotSetToCompositionType(composition, rs.Role.Resolve(), rs.SelfType)));
      return result;
    }

    private bool Matches(TypeReference selfType, TypeDefinition composition) {
      return TypeMatcher.IsMatch(composition.ResolveGenericArguments(), selfType);
    }

  }

}
