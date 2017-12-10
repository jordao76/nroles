# NRoles #

NRoles is an experiment with roles in C#. Roles, very similar to traits, are high level constructs that promote better code reuse through easier composition. NRoles is a post-compiler created with [Mono.Cecil](http://www.mono-project.com/Cecil) that transforms an assembly to enable roles and compositions.

## Synopsis ##

Reference the `NRoles.dll` (on the `NRoles` project output) assembly and create some roles and compositions:

```cs
using NRoles;

namespace Devices {

  public class RSwitchable : Role {
    private bool on = false;
    public void TurnOn() { on = true; }
    public void TurnOff() { on = false; }
    public bool IsOn { get { return on; } }
    public bool IsOff { get { return !on; } }
  }

  public class RTunable : Role {
    public int Channel { get; private set; }
    public void Seek(int step) { Channel += step; }
  }

  public class Radio : Does<RSwitchable>, Does<RTunable> { }

}

// somewhere in the same project...

var radio = new Radio();
radio.As<RSwitchable>().TurnOn();
radio.As<RTunable>().Seek(42);
```

Note: there are also other ways to [use compositions in the same project they're defined](WorkInTheSameProject.md).

Run the post-compiler `nutate.exe` (found on the `NRoles.App` project output) on the compiled assembly:

```sh
$ nutate Devices.dll
```

The roles are now composed in the compiled assembly.

When you reference it from another project:

```cs
// somewhere in some other project...

var radio = new Radio();
radio.TurnOn();
radio.Seek(42);
```

Roles cannot be instantiated, cannot inherit from any classes (only `Object`) and cannot have parameterized constructors. They can [implement interfaces](ImplementInterfaces.md), and these implementations will be carried over to the classes that compose them. They can also compose other roles and [declare self-types](http://codecrafter.blogspot.ca/2012/06/self-types-in-nroles.html). Conflicts in a composition must be [resolved explicitly](ResolveConflicts.md).

More examples with NRoles can be found [here](http://codecrafter.blogspot.com/2011/05/nroles-experiment-with-roles-in-c.html) and [here](http://codecrafter.blogspot.com/2011/06/dci-example-with-nroles.html).
