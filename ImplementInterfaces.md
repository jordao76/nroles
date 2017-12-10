# How to implement an interface through a role #

Roles can implement interfaces, and these implementations will be carried over to the classes that compose them.

## Example ##

Consider this interface:

```cs
public interface ISwitchable {
  void TurnOn();
  void TurnOff();
  bool IsOn { get; }
  bool IsOff { get; }
}
```

A role can encapsulate a common implementation for `ISwitchable`:

```cs
using NRoles;

public class RSwitchable : ISwitchable, Role {
  private bool on = false;
  public void TurnOn() { on = true; }
  public void TurnOff() { on = false; }
  public bool IsOn { get { return on; } }
  public bool IsOff { get { return !on; } }
}
```

A class can use that common implementation by composing the role:

```cs
public abstract class Device {
  public string Name { get; set; }
}

public class Light : Device, Does<RSwitchable> { }
```

This will make `Light` implement `ISwitchable` through `RSwitchable`.

In the same assembly you access `RSwitchable` like this:

```cs
var patioLight = new Light { Name = "Patio Light" };
// ...
patioLight.As<RSwitchable>().TurnOn();
// ...
MethodThatTakesISwitchable(patioLight.As<RSwitchable>());
```

After running the post-compiler, in another assembly you access `RSwichable` and `ISwitchable` directly:

```cs
patioLight.TurnOn();
// ...
MethodThatTakesISwitchable(patioLight);
```

A role can also be used _in lieu_ of an interface. This makes a role a kind of interface-with-code. A composition can reimplement any role members.

## Limitation ##

Currently, you cannot explicitly implement an interface in a role. You must use normal implicit interface implementations.
