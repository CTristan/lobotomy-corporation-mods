# Project Guidelines

## Architecture

This is a **Lobotomy Corporation mod collection** — standalone Harmony mods for a Unity game, built on .NET Framework 3.5. Each mod is an independent project that patches game classes via [Harmony](https://harmony.pardeike.net/).

### Project layout

| Project | Target | Purpose |
|---------|--------|---------|
| `LobotomyCorporationMods.Common` | net35 | Shared library: base classes, interfaces, extensions, facades, guard clauses |
| `LobotomyCorporationMods.<ModName>` | net35 | Individual mod — one per feature |
| `LobotomyCorporationMods.Test` | net481 | Single test project covering all mods |
| `CI` / `CI.Test` | net10.0 | Local dotnet tool for CI checks |
| `SetupExternal` / `SetupExternal.Test` | net10.0 | Local dotnet tool for project setup |
| `scripts/` | — | Helper shell/C# scripts for development workflows |

Game DLLs live in `external/LobotomyCorp_Data/Managed/` (not committed — set up via `dotnet setup`). The `external/decompiled/` directory contains decompiled game source for reference.

### Mod structure pattern

Every mod follows this structure:

```
ModName/
  Harmony_Patch.cs      # Entry point — sealed singleton extending HarmonyPatchBase
  Patches/              # One file per patched method
  Interfaces/           # (optional) Mod-specific interfaces
  Implementations/      # (optional) Mod-specific implementations
  Info/                 # Mod metadata XML
```

### BepInEx patcher projects

`RetargetHarmony` is a BepInEx **preloader patcher** (not a standard plugin). Key API differences from standard .NET:

- **`TypeLoader.AssemblyResolve`** is a BepInEx-specific event that uses **Mono.Cecil types**, NOT the standard .NET `AppDomain.AssemblyResolve`. Its delegate signature is:
  ```csharp
  AssemblyDefinition handler(object sender, AssemblyNameReference reference)
  ```
  This is correct — do NOT change it to `System.Reflection.ResolveEventArgs` / `Assembly`.
- **`Patch(AssemblyDefinition asm)`** receives Mono.Cecil `AssemblyDefinition` objects for IL-level modification before the runtime loads the assembly.

## Code Style

### File header

Every `.cs` file starts with `// SPDX-License-Identifier: MIT`.

### Harmony patch pattern (two-method design)

Each patch file contains a static class with exactly two methods:

1. **Testable business logic** — a public extension method on the original game type (e.g., `PatchAfterGetProb`)
2. **Harmony entry point** (`Postfix`/`Prefix`) — decorated with `[EntryPoint]` and `[ExcludeFromCodeCoverage]`, delegates to the extension method inside a try/catch that logs via `Harmony_Patch.Instance.Logger`

```csharp
[HarmonyPatch(typeof(OriginalClass), nameof(OriginalClass.Method))]
public static class OriginalClassPatchMethod
{
    public static ReturnType PatchAfterMethod(this OriginalClass instance, ...) { ... }

    [EntryPoint]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public static void Postfix(OriginalClass __instance, ref ReturnType __result)
    {
        try { __result = __instance.PatchAfterMethod(__result, ...); }
        catch (Exception ex) { Harmony_Patch.Instance.Logger.WriteException(ex); throw; }
    }
}
```

### Naming conventions

- **Patch classes**: `{OriginalClass}Patch{MethodName}` (e.g., `CreatureEquipmentMakeInfoPatchGetProb`)
- **Private fields**: `_camelCase`
- **Private static fields**: `s_camelCase`
- **Null checks**: `Guard.Against.Null(param, nameof(param))`
- **Harmony parameters**: Suppress `CA1707`/`IDE1006` with `// ReSharper disable InconsistentNaming`

### Key conventions

- `var` preferred everywhere
- Allman brace style (new line before `{`)
- `#region` blocks wrap `using` directives
- Extension methods on game types make patches testable
- `PrivateMethods` class holds string constants for non-public game methods patched via Harmony

## Build and Test

### Helper scripts

The `scripts/` directory contains workflow shortcuts (run from the repo root):

| Script | Purpose |
|--------|---------|
| `tool-reinstall.sh [name|all]` | Clean-build, repack, cache-clear, and reinstall local dotnet tools. No arguments reinstalls all (ci, playwright). Specify a name to reinstall just that tool. |
| `setup-reinstall.sh [name]` | Same workflow for the SetupExternal tool (default: `setup`) |
| `rename-assembly.csx` | Mono.Cecil script to rename a DLL's assembly name — usage: `dotnet script scripts/rename-assembly.csx <dll> <newName>` |

The reinstall scripts exist because `dotnet tool install --local` will silently use the NuGet cache instead of a freshly packed nupkg unless the cache is cleared first.

**Always run `./scripts/tool-reinstall.sh` after updating any local tool (CI or Playwright).**

```sh
# Initial setup (requires game installation + ilspycmd)
dotnet tool restore
dotnet setup              # Copies game DLLs to external/

# Build
dotnet build LobotomyCorporationMods.sln

# Test with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutput="./coverage.opencover.xml" /p:CoverletOutputFormat=opencover LobotomyCorporationMods.sln

# CI checks (format + test) — auto-fix mode
dotnet ci

# CI checks — verify mode (no auto-fix)
dotnet ci --check

# After updating local tools, reinstall them
./scripts/tool-reinstall.sh          # Reinstalls all local tools (ci, playwright)
./scripts/tool-reinstall.sh ci       # Reinstalls just ci
./scripts/tool-reinstall.sh playwright  # Reinstalls just playwright
```

### Testing conventions

- **Framework**: xUnit with FluentAssertions and Moq
- **Organization**: `LobotomyCorporationMods.Test/ModTests/{ModName}Tests/` mirrors each mod's structure
- **Base test classes**: Each mod has a shared base class providing common test setup
- **Test naming**: Descriptive sentences — `A_gift_that_has_not_been_worked_on_yet_displays_the_base_value`
- **Coverage target**: 100% patch coverage (enforced via Codecov)
- **Unity test helpers**: `UnityTestExtensions` provides factory methods for game objects
- **Every patch test verifies**: patch registration (`ValidateHarmonyPatch`), exception logging, and business logic

## Conventions

- Common's `AssemblyName` embeds its version (`LobotomyCorporationMods.Common.$(AssemblyVersion)`) to prevent Basemod DLL conflicts
- `TreatWarningsAsErrors` is enabled on all projects
- `.editorconfig` enforces formatting — run `dotnet format` or `dotnet ci` to auto-fix
- All external game DLL references use `<Private>false</Private>`
- NuGet packages for local tools are in `nupkg/` (configured via `nuget.config`)
- See [CONTRIBUTING.md](../CONTRIBUTING.md) for full development environment setup
