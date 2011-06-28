using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  /// <summary>
  /// The context for a mutation operation.
  /// </summary>
  public class MutationContext : IMessageContainer, Does<RMessageContainer> {

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="module">The module being mutated.</param>
    public MutationContext(ModuleDefinition module) {
      if (module == null) throw new ArgumentNullException("module");
      if (module.Assembly == null) throw new ArgumentException("module has a null Assembly", "module");
      Module = module;
      Assembly = module.Assembly;
    }

    /// <summary>
    /// The module being mutated.
    /// </summary>
    public ModuleDefinition Module { get; private set; }
    
    /// <summary>
    /// The assembly for the module being mutated.
    /// </summary>
    /// <seealso cref="Module"/>
    public AssemblyDefinition Assembly { get; private set; }


    /// <summary>
    /// Ends the context. Runs final operations: triggers the registered type and code visitors and
    /// executes the wrap up actions.
    /// </summary>
    /// <param name="result">The result of the operation. Will be used to add the context final messages.</param>
    /// <seealso cref="CodeVisitorsRegistry"/>
    /// <seealso cref="RegisterWrapUpAction"/>
    public void Finalize(IOperationResult result) {
      // TODO: shouldn't they visit the current module?
      _typeVisitors.Visit(Assembly);
      _codeVisitors.Visit(Assembly);
     _wrapUpActions.ForEach(action => action(this));
      result.Slurp(this); // transfer the messages to the result
    }

    #region Code Visitors

    ScopingCodeVisitor _codeVisitors = new ScopingCodeVisitor();

    /// <summary>
    /// The code visitors registry for this context.
    /// </summary>
    public ICodeVisitorRegistry CodeVisitorsRegistry {
      get { return _codeVisitors; }
    }

    #endregion

    #region Type Visitors

    CompositeAssemblyTypesVisitor _typeVisitors = new CompositeAssemblyTypesVisitor();

    /// <summary>
    /// The type visitors registry for this context.
    /// </summary>
    public IAssemblyTypesVisitorRegistry TypeVisitorsRegistry {
      get { return _typeVisitors; }
    }

    #endregion

    #region Wrap Up Actions

    List<Action<IMessageContainer>> _wrapUpActions = new List<Action<IMessageContainer>>();

    /// <summary>
    /// Registers a wrap up action to be run at the end of the mutation lifecycle.
    /// </summary>
    /// <param name="action">The action to register.</param>
    public void RegisterWrapUpAction(Action<IMessageContainer> action) {
      if (action != null) _wrapUpActions.Add(action);
    }

    #endregion

    #region Messages

    public extern IEnumerable<Message> Messages { [Placeholder] get; }
    [Placeholder] public extern void AddMessage(Message message);

    #endregion

  }

}
