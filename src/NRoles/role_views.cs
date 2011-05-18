using System;

namespace NRoles {

  /// <summary>
  /// A role view serves to resolve role member conflicts in a composition.
  /// </summary>
  /// <typeparam name="TRole">Role type to which the role view applies.</typeparam>
  public interface RoleView<TRole> : Role where TRole : Role { }

  /// <summary>
  /// Makes role members protected in a composition.
  /// </summary>
  [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
  public class GuardAttribute : Attribute { }

  /// <summary>
  /// Hide role members so that they don't conflict with other members with the same signature or name.
  /// To be used in role views (<see cref="RoleView`1"/>).
  /// </summary>
  /// <remarks>
  /// The member will be implemented as a private member in the composition, but will still
  /// be accessible through a reference to the role interface.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
  public class HideAttribute : Attribute { }

  /// <summary>
  /// Excludes a role member from a composition. Used in role views (<see cref="RoleView`1"/>) to 
  /// resolve role member conflicts.
  /// </summary>
  [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
  public class ExcludeAttribute : Attribute { }

  /// <summary>
  /// Aliases role members in a composition. Used in role views (<see cref="RoleView`1"/>) to 
  /// resolve role member conflicts.
  /// </summary>
  [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
  public class AliasingAttribute : Attribute {
    public readonly string MemberName;
    public AliasingAttribute(string memberName) {
      if (string.IsNullOrEmpty(memberName)) throw new ArgumentException("memberName cannot be null or empty");
      MemberName = memberName;
    }
  }

}
