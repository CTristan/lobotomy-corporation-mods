---
description: "Scaffold a complete new Lobotomy Corporation mod with Harmony_Patch.cs, .csproj, Info XML, and test base class"
agent: "agent"
---

Create a new mod project named `LobotomyCorporationMods.{{ModName}}` by following the established patterns in this workspace.

## Steps

1. **Create the project directory** `LobotomyCorporationMods.{{ModName}}/`

2. **Create `Harmony_Patch.cs`** following this exact pattern:

```csharp
// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.{{ModName}}
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.{{ModName}}.dll", initialize)
        {
        }
    }
}
```

3. **Create `.csproj`** — use [LobotomyCorporationMods.BugFixes/LobotomyCorporationMods.BugFixes.csproj](LobotomyCorporationMods.BugFixes/LobotomyCorporationMods.BugFixes.csproj) as a template. Set `AssemblyVersion` to `1.0.0` and generate a new `ProjectGuid`. Include the same external DLL references, PackageReference, and ProjectReference to Common.

4. **Create `Info/GlobalInfo.xml`**:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<info>
    <ID>{{ModName}}</ID>
</info>
```

5. **Create `Info/en/Info.xml`** with a `<name>` element containing the mod name and version, and `<descs>` elements describing what the mod does.

6. **Create empty `Patches/` directory** with a placeholder for future patch files.

7. **Create `README.md`** with a brief description of the mod.

8. **Add the project to `LobotomyCorporationMods.sln`** — add it to the solution file and add a ProjectReference from the test project.

9. **Create test base class** at `LobotomyCorporationMods.Test/ModTests/{{ModName}}Tests/{{ModName}}ModTests.cs` following the pattern in [LobotomyCorporationMods.Test/ModTests/FreeCustomizationTests/FreeCustomizationModTests.cs](LobotomyCorporationMods.Test/ModTests/FreeCustomizationTests/FreeCustomizationModTests.cs).

10. **Create `HarmonyPatchTests.cs`** at `LobotomyCorporationMods.Test/ModTests/{{ModName}}Tests/HarmonyPatchTests.cs` — initially empty, tests will be added when patches are created.

After scaffolding, run `dotnet build LobotomyCorporationMods.sln` to verify everything compiles.
