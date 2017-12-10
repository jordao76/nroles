## Working with compositions in the same project that they're defined ##

The fact that NRoles is a post-compiler makes it harder to code against compositions in the same project that they're defined.

## Example ##

Given this simple role:

```cs
public abstract class RPrintable : Role {
  public virtual void Print() {
    Console.WriteLine(ToString());
  }
}
```

And this composition:

```cs
public class Person : Does<RPrintable> {
  public string Name { get; set; }
  public string Profession { get; set; }
  public override string ToString() {
    return string.Format("{0} [{1}]", Name, Profession);
  }
}
```

This is the code that can be written after the post-compiler runs, in _another_ project:

```cs
var wilco = new Person { Name = "Roger Wilco", Profession = "space janitor" };
wilco.Print();
```

Let's see different ways to write this code in the same project that the composition is defined.

## Using the `As<TRole>` extension method ##

NRoles provides the `TRole As<TRole>()` extension method for this purpose:

```cs
var wilco = new Person { Name = "Roger Wilco", Profession = "space janitor" };
wilco.As<RPrintable>().Print();
```

This is safe but can make the code longer than necessary. It can be particularly unwieldy for generic roles. It casts the composition to a role it composes.

## Going dynamic ##

With C# 4, you can use `dynamic`:

```cs
dynamic wilco = new Person { Name = "Roger Wilco", Profession = "space janitor" };
wilco.Print();
```

It reads better but is not compile-time safe and also has negative performance implications.

## Placeholders ##

Members in a composition can be defined as placeholders that will be "filled up" by respective role members:

```cs
public class Person : Does<RPrintable> {
  ...
  [Placeholder] public void Print() { throw Away.Code; }
}
```

The `Print` method is marked with the `Placeholder` attribute. Its code doesn't really matter, it will be thrown away by the post-compiler. It can even be written as an external method (only with the MS C# compiler):

```cs
[Placeholder] public extern void Print();
```

The client code can simply be:

```cs
var wilco = new Person { Name = "Roger Wilco", Profession = "space janitor" };
wilco.Print();
```

Placeholders can be created only for the members that are used in the same project, but it's still a burden to create and maintain them.

## Interfaces and Placeholders ##

More practically, roles can implement interfaces, and the compositions can also implement those interfaces. Visual Studio can be used to provide default implementations for the interface, which can then be marked as placeholders. This scaffolding can be put in special `#region`s or on dedicated partial class files:

```cs
public interface IPrintable {
  void Print();
}

public abstract class RPrintable : IPrintable, Role {
  public virtual void Print() {
    Console.WriteLine(ToString());
  }
}

...

public partial class Person : IPrintable, Does<RPrintable> {
  [Placeholder] public void Print() { throw new NotImplementedException(); }
}
```
