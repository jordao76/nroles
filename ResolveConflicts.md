# How to resolve role conflicts #

When two or more roles are composed in a class and they have conflicting members, the conflict must be resolved. There are a couple of ways to resolve a conflict.

## Rationale ##

Conflicts should be _rare_, and when they happen a composition author _must_ resolve them explicitly. If the framework would resolve them implicitly, the final behavior of the composition could be different than what the author intended.

## Scenario ##

These roles will conflict when composed together:

```cs
public class R1 : Role {
  public void M() { Console.WriteLine("R1.M"); }
}
public class R2 : Role {
  public void M() { Console.WriteLine("R2.M"); }
}
```

This class will fail post-compilation with a conflict error message:

```cs
public class C : Does<R1>, Does<R2> { }
```

The following sections show different ways to resolve the conflict.

## Supercede the conflict in the composition ##

A composition can supercede the conflicting member:

```cs
public class C1 : Does<R1>, Does<R2> {
  public void M() { Console.WriteLine("C.M"); }
}
```

This will resolve the conflict with a member provided by the class itself, which will effectively supercede all role members that match the member type and signature.

This code:

```cs
C1 c = new C1();
c.As<R1>().M();
c.As<R2>().M();
c.M();
```

Will print:

```
C.M
C.M
C.M
```

## Exclude one of the conflicting members ##

A conflicting member can be excluded from the composition, what will make the remaining member win the conflict. To exclude a member, a _role view_ must be used:

```cs
interface R1View : RoleView<R1> {
  [Exclude] void M();
}
```

`R1View` is a role view for `R1` which excludes the method `void M()`.

Now the composition can be written like this:

```cs
public class C2 : Does<R1>, Does<R2>, Does<R1View> { }
```

And, since `R1.M` was excluded from the composition, this code:

```cs
C2 c = new C2();
c.As<R1>().M();
c.As<R2>().M();
```

Will print this:

```
R2.M
R2.M
```

If the role view is only used for that specific composition, it might make sense to create it as a nested type within the composition:

```cs
public class C2 : Does<R1>, Does<R2>, Does<C2.R1View> {
  interface R1View : RoleView<R1> {
    [Exclude] void M();
  }
}
```

## Alias one of the conflicting members ##

A role view can also be used to rename a role member:

```cs
public class C3 : Does<R1>, Does<R2>, Does<C3.R1View> {
  internal interface R1View : RoleView<R1> {
    [Aliasing("M")] void Aliased();
  }
}
```

`C3.R1View` aliases the method `R1.M` as `C3.Aliased`.

This code:

```cs
C3 c = new C3();
c.As<R1>().M();
c.As<R2>().M();
c.As<C3.R1View>().Aliased();
```

Will print:

```
R1.M
R2.M
R1.M
```

And after post-compilation, this code:

```cs
C3 c = new C3();
c.M();
c.Aliased();
```

Prints:

```
R2.M
R1.M
```

Aliased members can also be used when there're no conflicts to resolve. They can provide more meaningful names for a role's members in the context of specific compositions.
