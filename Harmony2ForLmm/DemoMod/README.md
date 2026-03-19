# DemoMod — Harmony 2 / BepInEx Feature Showcase

A complete, compilable showcase of every Harmony 2 and BepInEx feature available to Lobotomy Corporation modders through the Harmony 2 for LMM system.

## Features

| # | Feature | Tag | File |
|---|---------|-----|------|
| 1 | BepInEx plugin entry point | `[DemoMod:EntryPoint]` | `DemoMod.Mod/Plugin.cs` |
| 2 | Configuration system | `[DemoMod:Config]` | `DemoMod.Mod/Plugin.cs` |
| 3 | Config validation | `[DemoMod:ConfigValidation]` | `DemoMod.Mod/Plugin.cs` |
| 4 | MonoBehaviour lifecycle | `[DemoMod:Lifecycle]` | `DemoMod.Mod/Plugin.cs` |
| 5 | Finalizer patches | `[DemoMod:Finalizer]` | `DemoMod.Mod/Patches/AgentUpdateFinalizer.cs` |
| 6 | Reverse patches | `[DemoMod:ReversePatch]` | `DemoMod.Mod/Patches/XpFormulaReversePatch.cs` |
| 7 | Patch priority & ordering | `[DemoMod:Priority]` | `DemoMod.Mod/Patches/CreatureUpdatePriorityDemo.cs` |
| 8 | CodeMatcher transpiler | `[DemoMod:Transpiler]` | `DemoMod.Mod/Patches/SuccessCapTranspiler.cs` |
| 9 | Manual patching | `[DemoMod:ManualPatch]` | `DemoMod.Mod/Patches/EnergyMultiplierManualPatch.cs` |
| 10 | Dependency declarations | `[DemoMod:Dependencies]` | `DemoMod.Mod/Patches/WorkResultLogger.cs` |
| 11 | Preloader patchers | `[DemoMod:Preloader]` | `DemoMod.Mod/Patches/CreatureFieldVerifier.cs` + `DemoMod.Patcher/CreatureFieldPatcher.cs` |

## Guide cross-reference

Every source file contains `// Guide: §Section` comments linking code to the corresponding section in the [Modder's Guide](../Resources/docs/ModdersGuide.md). Look for `§` in any file to find the mapping.

## Prerequisites

- Game DLLs available via `dotnet setup` (copies to `external/LobotomyCorp_Data/Managed/`)
- .NET Framework 3.5 reference assemblies (via `Microsoft.NETFramework.ReferenceAssemblies.net35` NuGet)

## Build

From the repository root:

```bash
dotnet build Harmony2ForLmm/DemoMod/DemoMod.Mod/DemoMod.Mod.csproj
dotnet build Harmony2ForLmm/DemoMod/DemoMod.Patcher/DemoMod.Patcher.csproj
```

Or build the entire solution (includes DemoMod):

```bash
dotnet build LobotomyCorporationMods.sln
```

## Deployment

Deploy using the LobotomyPlaywright `demo` profile:

```bash
dotnet playwright deploy --profile demo
```

This copies:
- `DemoMod.Mod.dll` → `BaseMods/DemoMod.Mod/` (BepInEx plugin)
- `DemoMod.Patcher.dll` → `BepInEx/patchers/DemoMod.Patcher/` (preloader patcher)

## Testing

See [TESTING.md](TESTING.md) for step-by-step verification of all 11 features.
