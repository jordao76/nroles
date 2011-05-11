using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace NRoles.Engine {

  public class MemberResolver {
    // TODO: naming convention...?
    readonly TypeReference Target;
    readonly TypeDefinition Source;
    readonly GenericInstanceType TargetWithArguments;
    readonly ModuleDefinition Module;

    private GenericParametersMap _map;

    public MemberResolver(TypeReference target, ModuleDefinition targetModule = null) {
      if (target == null) throw new ArgumentNullException("target");
      Module = targetModule ?? target.Module;
      Target = target;
      Source = target.Resolve();
      TargetWithArguments = target as GenericInstanceType;
      _map = new GenericParametersMap(Source, TargetWithArguments);
    }

    public IMemberDefinition ResolveMemberDefinition(IMemberDefinition member) {
      var method = member as MethodDefinition;
      if (method != null) {
        return ResolveMethodDefinition(method);
      }

      var property = member as PropertyDefinition;
      if (property != null) {
        return ResolvePropertyDefinition(property);
      }

      var @event = member as EventDefinition;
      if (@event != null) {
        return ResolveEventDefinition(@event);
      }

      var field = member as FieldDefinition;
      if (field != null) {
        return ResolveFieldDefinition(field);
      }

      throw new InvalidOperationException();
    }

    public MethodDefinition ResolveMethodDefinition(MethodDefinition sourceMethod) {
      return ResolveMethodDefinition(sourceMethod, sourceMethod.Name, sourceMethod.Attributes);
    }    
    
    public MethodDefinition ResolveMethodDefinition(MethodDefinition sourceMethod, string name, MethodAttributes methodAttributes) {
      if (sourceMethod == null) throw new ArgumentNullException("sourceMethod");

      bool createdMap = false;
      if (sourceMethod.HasGenericParameters) {
        createdMap = true;
        _map = new GenericParametersMap(sourceMethod) { Next = _map };
      }

      var returnType = ResolveConstituentType(sourceMethod.ReturnType);
      var targetMethod = new MethodDefinition(
        name,
        methodAttributes,
        returnType);
      targetMethod.SemanticsAttributes = sourceMethod.SemanticsAttributes;

      ResolveGenericParameters(sourceMethod, targetMethod);
      ResolveParameters(sourceMethod, targetMethod);

      if (createdMap) {
        _map = _map.Next;
      }

      return targetMethod;
    }

    public PropertyDefinition ResolvePropertyDefinition(PropertyDefinition sourceProperty) {
      if (sourceProperty == null) throw new ArgumentNullException("sourceProperty");
      return new PropertyDefinition(
        sourceProperty.Name,
        sourceProperty.Attributes,
        ResolveConstituentType(sourceProperty.PropertyType));
    }

    public EventDefinition ResolveEventDefinition(EventDefinition sourceEvent) {
      if (sourceEvent == null) throw new ArgumentNullException("sourceEvent");
      return new EventDefinition(
        sourceEvent.Name,
        sourceEvent.Attributes,
        ResolveConstituentType(sourceEvent.EventType));
    }

    public FieldDefinition ResolveFieldDefinition(FieldDefinition sourceField) {
      if (sourceField == null) throw new ArgumentNullException("sourceField");
      return new FieldDefinition(
        sourceField.Name,
        sourceField.Attributes,
        ResolveConstituentType(sourceField.FieldType));
    }

    private void ResolveParameters(MethodDefinition sourceMethod, MethodReference targetMethod) {
      foreach (ParameterDefinition sourceParameter in sourceMethod.Parameters) {
        var targetParameter = new ParameterDefinition(
          sourceParameter.Name,
          sourceParameter.Attributes,
          ResolveConstituentType(sourceParameter.ParameterType));

        // copy the params attribute // TODO: is this necessary?
        foreach (CustomAttribute attribute in sourceParameter.CustomAttributes) {
          if (attribute.Constructor.DeclaringType.Resolve() == sourceMethod.DeclaringType.Module.Import(typeof(ParamArrayAttribute)).Resolve()) {
            targetParameter.CustomAttributes.Add(attribute);
          }
        }

        targetMethod.Parameters.Add(targetParameter);
      }
    }

    private void ResolveGenericParameters(IGenericParameterProvider source, IGenericParameterProvider target) {
      // TODO: merge with extension method CopyGenericParametersFrom
      if (source.HasGenericParameters) {
        foreach (GenericParameter sourceParameter in source.GenericParameters) {
          var targetParameter = new GenericParameter(sourceParameter.Name, target);
          targetParameter.CopyGenericConstraintsFrom(sourceParameter);
          target.GenericParameters.Add(targetParameter);
        }
      }
    }

    private void ResolveGenericParametersAsReferences(IGenericParameterProvider source, IGenericParameterProvider target) {
      if (source.HasGenericParameters) {
        foreach (GenericParameter parameter in source.GenericParameters) {
          target.GenericParameters.Add(
            new GenericParameter("!!" + parameter.Position, target));
        }
      }
    }

    private void ResolveGenericParametersAsArguments(IGenericParameterProvider source, IGenericInstance target) {
      if (source.HasGenericParameters) {
        foreach (GenericParameter parameter in source.GenericParameters) {
          target.GenericArguments.Add(parameter);
        }
      }
    }

    public MethodReference ResolveMethodReference(MethodReference sourceMethod, bool @override = false) {
      if (sourceMethod == null) throw new ArgumentNullException("sourceMethod");

      sourceMethod = Module.Import(sourceMethod);

      if (!sourceMethod.HasGenericParameters && (TargetWithArguments == null || !TargetWithArguments.HasGenericArguments)) {
        return sourceMethod;
      }

      var declaringType = ResolveMatchingType(sourceMethod.DeclaringType);
      var returnType = ResolveMatchingType(sourceMethod.ReturnType);
      var targetMethod = new MethodReference(
        sourceMethod.Name,
        returnType) {
          DeclaringType = declaringType,
          HasThis = sourceMethod.HasThis,
          ExplicitThis = sourceMethod.ExplicitThis,
          CallingConvention = sourceMethod.CallingConvention
        };
      if (sourceMethod.HasGenericParameters) {
        ResolveGenericParametersAsReferences(sourceMethod, targetMethod);
        if (!@override) {
          targetMethod = new GenericInstanceMethod(targetMethod);
          ResolveGenericParametersAsArguments(sourceMethod, (GenericInstanceMethod)targetMethod);
        }
      }
      foreach (ParameterDefinition parameter in sourceMethod.Parameters) {
        targetMethod.Parameters.Add(
          new ParameterDefinition(parameter.Name, parameter.Attributes,
            ResolveMatchingType(parameter.ParameterType)));
      }

      return targetMethod;
    }

    public FieldReference ResolveFieldReference(FieldDefinition sourceField) {
      if (sourceField == null) throw new ArgumentNullException("sourceField");
      return new FieldReference(
        sourceField.Name,
        sourceField.FieldType) {
          DeclaringType = ResolveMatchingType(sourceField.DeclaringType)
        };
    }

    public TypeReference ResolveMatchingType(TypeReference sourceType) {
      if (sourceType == null) throw new ArgumentNullException("sourceType");

      if (!sourceType.HasGenericParameters) {
        return sourceType;
      }
      if (TargetWithArguments == null || !TargetWithArguments.HasGenericArguments) {
        return sourceType;
      }
      if (sourceType.GenericParameters.Count != TargetWithArguments.GenericArguments.Count) {
        throw new InvalidOperationException();
      }

      var targetType = new GenericInstanceType(sourceType);
      foreach (TypeReference argument in TargetWithArguments.GenericArguments) {
        targetType.GenericArguments.Add(argument);
      }

      return targetType;
    }

    private TypeReference Import(TypeReference type) {
      if (type is TypeSpecification || type is GenericParameter) {
        return type;
      }
      if (type.Module != Module) {
        return Module.Import(type);
      }
      return type;
    }

    public TypeReference ResolveConstituentType(TypeReference sourceType) {
      if (sourceType == null) throw new ArgumentNullException("sourceType");

      sourceType = Import(sourceType);

      if (sourceType is GenericParameter) {
        return _map[sourceType.Name];
      }

      {
        var sourceTypeAsGenericInstance = sourceType as GenericInstanceType;
        if (sourceTypeAsGenericInstance != null) {
          var targetType = new GenericInstanceType(sourceTypeAsGenericInstance.ElementType);
          foreach (TypeReference sourceArgument in sourceTypeAsGenericInstance.GenericArguments) {
            var targetArgument = ResolveConstituentType(sourceArgument);
            targetType.GenericArguments.Add(targetArgument);
          }
          return targetType;
        }
      }

      {
        var sourceTypeAsArray = sourceType as ArrayType;
        if (sourceTypeAsArray != null) {
          return new ArrayType(ResolveConstituentType(sourceTypeAsArray.ElementType));
        }
      }

      {
        var sourceTypeAsReference = sourceType as ByReferenceType;
        if (sourceTypeAsReference != null) {
          return new ByReferenceType(ResolveConstituentType(sourceTypeAsReference.ElementType));
        }
      }

      {
        var sourceTypeAsOptional = sourceType as OptionalModifierType;
        if (sourceTypeAsOptional != null) {
          return new OptionalModifierType(ResolveConstituentType(sourceTypeAsOptional.ElementType), ResolveConstituentType(sourceTypeAsOptional.ModifierType));
        }
      }

      {
        var sourceTypeAsRequired = sourceType as RequiredModifierType;
        if (sourceTypeAsRequired != null) {
          return new RequiredModifierType(ResolveConstituentType(sourceTypeAsRequired.ElementType), ResolveConstituentType(sourceTypeAsRequired.ModifierType));
        }
      }

      // TODO:
      //FunctionPointerType?
      //SentinelType??

      // PinnedType is never used as a parameter (TODO: or is it?)
      // PointerType never has a generic element type

      return sourceType;
    }

  }

  class GenericParametersMap {

    private IGenericParameterProvider _source;
    private Dictionary<string, TypeReference> _map = new Dictionary<string, TypeReference>();

    public GenericParametersMap(IGenericParameterProvider source, IGenericInstance target) {
      if (source == null) throw new ArgumentNullException("source");
      _source = source;
      if (target == null) return;
      if (!target.HasGenericArguments) return;
      if (source.GenericParameters.Count != target.GenericArguments.Count) throw new ArgumentException();

      int position = 0;
      foreach (TypeReference argument in target.GenericArguments) {
        var parameter = source.GenericParameters[position];
        _map.Add(parameter.Name, argument);
        ++position;
      }
    }

    public GenericParametersMap(IGenericParameterProvider source) {
      if (source == null) throw new ArgumentNullException("source");
      _source = source;
      if (!source.HasGenericParameters) return;

      foreach (GenericParameter parameter in source.GenericParameters) {
        _map.Add(parameter.Name, parameter);
      }
    }

    public GenericParametersMap Next { get; set; }

    public TypeReference this[string genericParameterName] {
      get {
        TypeReference argument;
        if (_map.TryGetValue(genericParameterName, out argument)) {
          return argument;
        }
        if (Next != null) {
          return Next[genericParameterName];
        }
        // the parameter was not found, return a generic type reference
        return new GenericParameter(genericParameterName, _source);
      }
    }

  }

}
