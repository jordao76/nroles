using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NRoles.Engine {

  public class RoleComposerAssemblyMutator : IMutator {

    public IOperationResult Mutate(MutationParameters parameters) {
      parameters.Validate();
      var assembly = parameters.Assembly;
      var result = new CompositeOperationResult();
      assembly.MainModule.GetAllTypes(). // TODO: what about other modules?
        Where(type => DoesRoles(type)).
        // TODO: do we need a special order here? like base classes first?
        ForEach(type => {
          var singleResult = new RoleComposerMutator().ComposeRoles(
            new MutationParameters {
              SourceType = type,
              Context = parameters.Context
            });
          result.AddResult(singleResult);
        });
      return result;
    }

    private bool DoesRoles(TypeDefinition targetType) {
      return targetType.Interfaces.
        Any(@interface => @interface.DoesRole());
    }
  
  }

}
