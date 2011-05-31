using System;

namespace NRoles {

  /// <summary>
  /// Used to mark an assembly so as to not be mutated by NRoles.
  /// Also used by the NRoles engine to mark a mutated assembly so that it's not mutated again.
  /// </summary>
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
  public class DontMutateAttribute : Attribute { }

}
