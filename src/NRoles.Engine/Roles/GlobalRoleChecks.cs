using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NRoles.Engine {

  public class GlobalRoleChecks : IMutator {

    public IOperationResult Mutate(MutationParameters parameters) {
      parameters.Validate();
      return Check(parameters);
    }

    public IOperationResult Check(MutationParameters parameters) {
      return Check(parameters.Context);
    }

    public IOperationResult Check(MutationContext context) {
      var result = new OperationResult();

      CheckNoClassesImplementRolesDirectly(context, result);

      return result;
    }

    private void CheckNoClassesImplementRolesDirectly(MutationContext context, OperationResult result) {
      context.TypeVisitorsRegistry.Register(new FindRolesImplementations(context));
    }

  }

}
