using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NRoles.Engine {

  /// <summary>
  /// Common operations for mutators that extract a type from another type.
  /// </summary>
  public abstract class ExtractTypeMutator {

    /// <summary>
    /// Extracts a type from another type and returns the result of the operation.
    /// </summary>
    /// <param name="parameters">Parameters to the operation.</param>
    /// <returns>Result of the operation.</returns>
    public ExtractTypeResult ExtractType(ExtractTypeParameters parameters) {
      parameters.Validate();
      var sourceType = parameters.SourceType;

      var result = new ExtractTypeResult();
      sourceType.Accept(CreateVisitor(parameters, result));

      if (result.Success) {
        if (parameters.AddTypeStrategy == AddTypeStrategy.AddAsNested) {
          sourceType.NestedTypes.Add(result.TargetType);
        }
      }

      return result;
    }

    /// <summary>
    /// Creates a <see cref="ITypeVisitor"/> that will visit the source type given in the 
    /// <paramref name="parameters"/> and extract the target type, which will be set in the
    /// <paramref name="result"/>
    /// </summary>
    /// <param name="parameters">Parameters to the type extraction.</param>
    /// <param name="result">Result of the type extraction.</param>
    /// <returns>Visitor that performs the type extraction.</returns>
    /// <remarks>
    /// Use the class <see cref="ExtractTypeMutatorVisitor"/> as a base to easily create a 
    /// visitor.
    /// </remarks>
    protected abstract ITypeVisitor CreateVisitor(ExtractTypeParameters parameters, ExtractTypeResult result);

    /// <summary>
    /// Base class for type visitors that extract a target type from a source type.
    /// </summary>
    protected abstract class ExtractTypeMutatorVisitor : TypeVisitorBase {

      /// <summary>
      /// Parameters to the type extraction.
      /// </summary>
      protected ExtractTypeParameters Parameters { get; set; }
      
      /// <summary>
      /// Result of the type extraction.
      /// </summary>
      protected ExtractTypeResult Result { get; set; }
      
      /// <summary>
      /// Name of the target type. Taken from the <see cref="Parameters"/>.
      /// </summary>
      protected string TargetTypeName { get { return Parameters.TargetTypeName; } }

      /// <summary>
      /// Extracted target type. Set in the <see cref="Result"/>.
      /// </summary>
      protected TypeDefinition TargetType {
        get { return Result.TargetType; }
        set { Result.TargetType = value; }
      }

      /// <summary>
      /// Source type for the extraction. Taken from the <see cref="Parameters"/>.
      /// </summary>
      protected TypeDefinition SourceType { get { return Parameters.SourceType; } }

      /// <summary>
      /// Creates a new instance of this class.
      /// </summary>
      /// <param name="parameters">Parameters to the type extraction.</param>
      /// <param name="result">Result of the type extraction.</param>
      /// <remarks>
      /// This class will visit the source type in the <paramref name="parameters"/>,
      /// and extract a target type that will be stored in the <paramref name="result"/>.
      /// </remarks>
      protected ExtractTypeMutatorVisitor(ExtractTypeParameters parameters, ExtractTypeResult result) {
        Parameters = parameters;
        Result = result;
      }

      /// <summary>
      /// Creates a default constructor on the target type that calls the <see cref="Object"/>
      /// class' constructor.
      /// </summary>
      protected void CreateDefaultConstructor() {
        new DefaultConstructorGenerator(TargetType, SourceType.Module).CreateConstructor();
      }

    }

  }

}

