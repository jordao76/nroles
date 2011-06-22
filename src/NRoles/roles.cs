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
  /// Marks a role member as superseded in a composition type.
  /// </summary>
  [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
  public class SupersedeAttribute : Attribute { }

  /// <summary>
  /// Marks a role member as a placeholder in a composition type. 
  /// The composition member will be replaced by the member from the role.
  /// </summary>
  [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
  public class PlaceholderAttribute : Attribute { }

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
    /// Cast a role to another type. Note: this cast can fail.
    /// Only necessary when using roles in the same assembly that
    /// they are defined, because the post-compiler hasn't yet run on them.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <param name="self">Role to cast from.</param>
    /// <returns>The input role instance cast to the desired type.</returns>
    public static T Cast<T>(this Role self) {
      return (T)self;
    }

  }

  /// <summary>
  /// Defines a single method to be used in a placeholder (<see cref="PlaceholderAttribute"/>).
  /// It's just a convenient way to mark it as throw-away code: <c>throw Away.Code</c>.
  /// </summary>
  public static class Away {
    
    /// <summary>
    /// To be used as a placeholder's code simply as <c>throw Away.Code</c>.
    /// </summary>
    public static Exception Code {
      get {
        return new Exception("Throw-away code not thrown away!");
      }
    }

  }

}
