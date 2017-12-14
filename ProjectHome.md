# NRoles

[![NuGet version](http://img.shields.io/nuget/v/NRoles.svg)](https://www.nuget.org/packages/NRoles/)
[![AppVeyor build](https://img.shields.io/appveyor/ci/jordao76/nroles.svg)](https://ci.appveyor.com/project/jordao76/nroles)
[![License](http://img.shields.io/:license-mit-blue.svg)](https://github.com/jordao76/nroles/blob/master/LICENSE.txt)

NRoles is an experiment with roles in C#. Roles, very similar to traits, are high level constructs that promote better code reuse through easier composition. NRoles is a post-compiler created with [Mono.Cecil](http://www.mono-project.com/Cecil) that transforms an assembly to enable roles and compositions.

## Installation

NRoles is available as a NuGet package. Once installed in a project, it will add a custom MSBuild post-compilation step that will transform the target assembly to enable roles and compositions.

## Example

_Roles_ are classes marked with the interface `NRoles.Role`, and _compositions_ with the interface `NRoles.Does<TRole>`, with the generic parameter `TRole` set to a role:

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

Note that there are also other ways to [use compositions in the same project they're defined](WorkInTheSameProject.md).

When you reference it from another project:

```cs
// somewhere in some other project...

var radio = new Radio();
radio.TurnOn();
radio.Seek(42);
```

Roles cannot be instantiated, cannot inherit from any classes (only `Object`) and cannot have parameterized constructors. They can [implement interfaces](ImplementInterfaces.md), and these implementations will be carried over to the classes that compose them. They can also compose other roles and [declare self-types](SelfTypes.md). Conflicts in a composition must be [resolved explicitly](ResolveConflicts.md).

More examples with NRoles can be found [here](http://codecrafter.blogspot.com/2011/05/nroles-experiment-with-roles-in-c.html) and [here](http://codecrafter.blogspot.com/2011/06/dci-example-with-nroles.html).

## Disclaimer

NRoles is an _experiment_, it can perform extensive changes to target assemblies, and might not work well with other tools and processes, most notably _debugging_.

## License

Licensed under the [MIT license](https://github.com/jordao76/nroles/blob/master/LICENSE.txt).
