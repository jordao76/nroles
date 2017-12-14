## Self-types

Since roles are composed into target classes, it's sometimes necessary for them to be able to identify the type of these classes, what is called a _self-type_.

Self-types are enabled by the presence of a type parameter with the special name `S` on a role:

```cs
// S is the self-type
public abstract class REquatable<S> : IEquatable<S>, Role {
  public abstract bool Equals(S other);
  public bool Differs(S other) {
    return !Equals(other);
  }
}
```

To cast `this` to the self-type, use the `Cast<T>` extension method:

```cs
public class Identity<S> : Role {
  public S Self {
    get { return this.Cast<S>(); }
  }
}
```

When other roles compose roles with self-types, they _must_ flow the self-type through, since it will ultimately be determined by the target classes that compose them:

```cs
public abstract class RComparable<S> :
  IComparable<S>, Does<REquatable<S>>, Role {
  ...
}
```

All classes that compose roles with self-types will be checked and an error message will be issued if the type parameter is not the target class itself.

```cs
public sealed class Money :
  // must use Money as the self-type parameter
  Does<RComparable<Money>>, Does<REquatable<Money>> {
  ...
}
```

Constraints on the self-type become _requirements_ for the target classes to fulfill:

```cs
public class MyRole<S> : Role
  // requires composing classes to implement ISomeInterface
  where S : ISomeInterface {
  ...
}

// MyClass must implement ISomeInterface
public class MyClass : ISomeInterface,
  Does<MyRole<MyClass>> {
  ...
}
```

This way, roles can work with the type of the classes that compose them generically and safely. As another example, it can be used for _fluent interfaces_ that return the type of `this`, used for method chaining:

```cs
public class PropertyBag<S> : Role {
  public S With(string name, object value) {
    ...
    return this.Cast<S>();
  }
}
```
