using System;

namespace NRoles {
  
  /// <summary>
  /// Marks a type as a role.
  /// </summary>
  public interface Role { } 

  /// <summary>
  /// Composes a role into a type.
  /// </summary>
  /// <typeparam name="TRole">Role to compose.</typeparam>
  public interface Does<TRole> where TRole : Role { }

  /// <summary>
  /// Supercedes role members in target types.
  /// </summary>
  [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
  public class SupersedeAttribute : Attribute { }

  /// <summary>
  /// Extension methods to aid in developing with roles in the same assembly, where
  /// the post-compiler hasn't run yet.
  /// </summary>
  public static class Roles {
    
    /// <summary>
    /// Treats a composition type as one of the roles it composes.
    /// Only necessary when using compositions in the same assembly that
    /// they are defined, because the post-compiler hasn't yet run on them.
    /// </summary>
    /// <typeparam name="TRole">The role that the composition uses.</typeparam>
    /// <param name="self">The composition.</param>
    /// <returns>The composition cast to the desired role.</returns>
    public static TRole As<TRole>(this Does<TRole> self) where TRole : Role {
      return (TRole)self;
    }

    /// <summary>
    /// Cast one role to another. Note: this cast can fail.
    /// Only necessary in the same assembly that the roles
    /// are defined, because the post-compiler hasn't yet run on them.
    /// </summary>
    /// <typeparam name="TRole">Target role type.</typeparam>
    /// <param name="self">Role to cast from.</param>
    /// <returns>The input role instance cast to the desired type.</returns>
    public static TRole Cast<TRole>(this Role self) where TRole : Role {
      return (TRole)self;
    }

  }

}
