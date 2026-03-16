# Project Guidelines

## Architecture

**Lobotomy Corporation mod collection** — standalone Harmony mods for a Unity game, built on .NET Framework 3.5. Each mod patches game classes via [Harmony](https://harmony.pardeike.net/).

### Project layout

| Project | Target | Purpose |
|---------|--------|---------|
| `LobotomyCorporationMods.Common` | net35 | Shared library: base classes, interfaces, extensions, facades |
| `LobotomyCorporationMods.<ModName>` | net35 | Individual mod — one per feature |
| `LobotomyCorporationMods.Test` | net10.0 | Single test project covering all mods |
| `CI` / `CI.Test` | net10.0 | Local dotnet tool for CI checks |
| `SetupExternal` / `SetupExternal.Test` | net10.0 | Local dotnet tool for project setup |
| `scripts/` | — | Helper shell/C# scripts for development workflows |

Game DLLs: `external/LobotomyCorp_Data/Managed/` (not committed — `dotnet setup`). Decompiled source: `external/decompiled/Assembly-CSharp/`.

### Mod structure

Each mod has: `Harmony_Patch.cs` (sealed singleton extending `HarmonyPatchBase`), `Patches/` (one file per patched method), optional `Interfaces/`, `Implementations/`, and `Info/` (metadata XML).

### BepInEx patcher (RetargetHarmony)

`RetargetHarmony` is a BepInEx **preloader patcher** using Mono.Cecil types:
- `TypeLoader.AssemblyResolve` uses delegate `AssemblyDefinition handler(object sender, AssemblyNameReference reference)` — do NOT change to `System.Reflection.ResolveEventArgs` / `Assembly`
- `Patch(AssemblyDefinition asm)` receives Mono.Cecil objects for IL modification

### Key conventions

- Common's `AssemblyName` embeds its version (`LobotomyCorporationMods.Common.$(AssemblyVersion)`) to prevent Basemod DLL conflicts
- All external game DLL references use `<Private>false</Private>`
- NuGet packages for local tools are in `nupkg/` (configured via `nuget.config`)

## Code Style

- Every `.cs` file starts with `// SPDX-License-Identifier: MIT`
- `var` preferred everywhere; Allman brace style; `#region` blocks wrap `using` directives
- **Private fields**: `_camelCase`; **private static fields**: `s_camelCase`
- **Null checks**: `ThrowHelper.ThrowIfNull(param)` — uses `CallerArgumentExpression` to auto-capture parameter name
- **Harmony parameters**: Suppress `CA1707`/`IDE1006` with `// ReSharper disable InconsistentNaming`
- `PrivateMethods` class holds string constants for non-public game methods patched via Harmony

### Harmony patch pattern (two-method design)

Each patch file has a static class named `{OriginalClass}Patch{MethodName}` with exactly two methods:

1. **Testable business logic** — public extension method on the game type (e.g., `PatchAfterGetProb`)
2. **Harmony entry point** (`Postfix`/`Prefix`) — `[EntryPoint]` + `[ExcludeFromCodeCoverage]`, delegates to the extension method inside try/catch logging via `Harmony_Patch.Instance.Logger.WriteException(ex)`

See existing patch files for the full template.

## Build and Test

### Helper scripts

| Script | Purpose |
|--------|---------|
| `tool-reinstall.sh [name|all]` | Clean-build, repack, cache-clear, and reinstall local dotnet tools. No arguments reinstalls all (ci, playwright). |
| `setup-reinstall.sh [name]` | Same workflow for SetupExternal tool (default: `setup`) |
| `rename-assembly.csx` | Mono.Cecil script to rename a DLL's assembly name |

**Always run `./scripts/tool-reinstall.sh` after updating any local tool (CI or Playwright).**

```sh
dotnet tool restore && dotnet setup    # Initial setup
dotnet build LobotomyCorporationMods.sln
dotnet test /p:CollectCoverage=true /p:CoverletOutput="./coverage.opencover.xml" /p:CoverletOutputFormat=opencover LobotomyCorporationMods.sln
dotnet ci                              # Auto-fix format + test
dotnet ci --check                      # Verify mode (no auto-fix)
```

## Planning

Plan documents track ongoing and future work. See [`PLANS.md`](../PLANS.md) for the full index with statuses and progress.

- Plan files live at the repo root, named `PLAN-<Topic>.md` in PascalCase
- Statuses: **Draft** | **Active** | **Blocked** | **Completed** | **Archived**
- When creating a new plan via `/planning`, use the `PLAN-<Topic>.md` naming convention and add an entry to `PLANS.md`
- When completing plan tasks, update the progress fraction in `PLANS.md`

## Constructor Parameter Rules

Every class falls into exactly one category. No exceptions.

**Category 1 — Fully Testable**: Constructor parameters are exclusively primitives (`string`, `int`, `long`, `float`, `double`, `decimal`, `bool`, `char`, `byte`, enums, `Type`) or interfaces. Must have 100% coverage, must NOT have `[ExcludeFromCodeCoverage]`.

**Category 2 — Boundary Wrappers**: Constructor includes at least one concrete Unity/game type. Must have `[AdapterClass]` + `[ExcludeFromCodeCoverage]`, contain minimal logic (delegation only), and bridge concrete types behind interfaces. **Never use Unity types or game types inheriting from `UnityEngine.Object` in Category 1 classes** — they throw `ECall` errors outside the Unity runtime. Access them only through interfaces. Use `ILogger`/`ILoggerTarget` from Common instead of `Debug.Log`.

When business logic lives in a class that takes concrete types, refactor: create an interface, create a thin Category 2 wrapper, change the business-logic class to accept the interface.

## csproj Conventions

### Shared properties (in `Directory.Build.props` — do NOT duplicate in `.csproj`)

`AnalysisLevel`, `Deterministic`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, `TreatWarningsAsErrors`, `OutputPath`, `DebugType`, `CheckForOverflowUnderflow`, `WarningLevel`, Roslynator analyzer packages.

### Format

- SDK-style: `<Project Sdk="Microsoft.NET.Sdk">` — no XML declaration, no `xmlns`/`ToolsVersion`, no `.ruleset` references
- 2-space indentation; `.editorconfig` enforces formatting (`dotnet format` or `dotnet ci` to auto-fix)
- All non-net35 projects must have `<Nullable>enable</Nullable>`

### What belongs in a `.csproj`

`TargetFramework`, `AssemblyVersion`/`Version`, `OutputType`, tool packaging props, `ProjectGuid`, `Nullable`, project-specific `<NoWarn>` (NU-prefixed only), package/project/DLL references, content/resource includes.

## Interface and Implementation Conventions

- Interfaces in `Interfaces/` folder, implementations in `Implementations/`; namespaces match folder segments
- **Naming**: `I` prefix for interfaces; implementations use same root name without `I`
- **Visibility**: `public` for shared contracts in Common, `internal` for mod-private. Prefer `sealed` for leaf implementations. **Never** use `InternalsVisibleTo`.
- Use interfaces for injected/mocked seams. May internally instantiate Category 1 classes only; Category 2 wrappers must enter via interface parameter.
- Sentinel/no-op implementations may skip base class inheritance (e.g., `NoneEvaluator : ICreatureEvaluator`)

## JsonUtility Serialization Rules

Based on [Unity 2017.4 JsonSerialization docs](https://docs.unity3d.com/2017.4/Documentation/Manual/JSONSerialization.html). JsonUtility uses Unity's serializer internally, so all [Unity serialization rules](https://docs.unity3d.com/2017.4/Documentation/Manual/script-Serialization.html) apply.

### Folder convention

All JsonUtility data classes **must** live in a `JsonModels/` folder (`.editorconfig` suppresses rules via `[**/JsonModels/**.cs]`). Do not place non-JsonUtility classes there.

### Class requirements

- Must have `[Serializable]` attribute
- Must not be `abstract`, `static`, or generic (but may inherit from a generic base class)
- Structs are serialized by value

### Field requirements

- Must be `public`, OR `private`/`protected` with `[SerializeField]`
- Must NOT be `static`, `const`, or `readonly`
- `[NonSerialized]` excludes a field from serialization
- Only **fields** are serialized — **properties are ignored**

### Supported field types

| Category | Types |
|----------|-------|
| Primitives | `string`, `int`, `float`, `double`, `bool`, `long`, `byte`, `char`, enums |
| Containers | `T[]` (arrays) and `List<T>` of any supported type |
| Nested objects | `[Serializable]` classes/structs (serialized inline by value) |
| Unity built-ins | `Vector2`, `Vector3`, `Vector4`, `Rect`, `Quaternion`, `Matrix4x4`, `Color`, `Color32`, `LayerMask`, `AnimationCurve`, `Gradient`, `RectOffset`, `GUIStyle` |

### Unsupported types (silently ignored or cause errors)

- `Dictionary<K,V>` — silently ignored, field will be `null`/default
- `object` fields — no type information preserved
- Multidimensional arrays (`T[,]`), jagged arrays (`T[][]`), nested containers (`List<List<T>>`, `List<T[]>`)
- Interfaces as field types
- Anonymous objects (not `[Serializable]`, serialize as `{}`)

### Critical behavioral gotchas

- **No null for custom objects**: Unity auto-instantiates new objects for null fields of custom `[Serializable]` types. Can cause unexpected allocations; recursive types can cause infinite loops and stack overflows.
- **No polymorphism**: Fields serialize by **declared** type, not runtime type. A field typed `Animal` holding a `Dog` deserializes as `Animal`, losing `Dog`-specific data.
- **Inline by-value (no reference preservation)**: Custom class references are serialized inline like structs. Two fields pointing to the same object become two separate copies after round-trip.
- **7-level depth limit**: Serializer stops processing nested custom classes/structs/lists/arrays beyond 7 levels deep.
- **Missing JSON fields**: C# field keeps its default value.
- **Extra JSON fields**: Silently ignored.
- **MonoBehaviour/ScriptableObject**: Cannot use `FromJson()` — must use `FromJsonOverwrite()`.

### Project patterns

**Placeholder pattern** for polymorphic/dynamic data: Use a `string` placeholder field + `[NonSerialized] object` field, serialize separately and string-replace. See `MessageSerializer.cs` for reference implementation.

**PascalCase accessors**: Lowercase public fields for serialization, optional PascalCase property accessors for C# code ergonomics. Properties are not serialized by JsonUtility so they do not affect the JSON output.

## Test Conventions

- **Framework**: xUnit + AwesomeAssertions (never raw `Assert`) + Moq
- **Organization**: Test files must be in subfolders, never in the test project root. Mod tests in `ModTests/{ModName}Tests/`, patch tests in `PatchTests/`. Each mod has a `{ModName}ModTests` base class.
- **Naming**: Descriptive sentences with underscores (e.g., `A_gift_that_has_not_been_worked_on_yet_displays_the_base_value`)
- **Unity objects**: Use `UnityTestExtensions` factory methods — never `new` Unity types directly
- **Mocking**: `TestExtensions.GetMockLogger()`, verify via `mockLogger.VerifyArgumentNullException(Action)`
- **Coverage**: 100% patch coverage enforced via Codecov. Test extension methods, not `[ExcludeFromCodeCoverage]` entry points.
- **Every patch must have**: (1) registration test via `ValidateHarmonyPatch`, (2) exception logging test, (3) business logic tests calling the extension method directly

## Warning Suppression Rules

**First priority: fix the code** so the warning no longer fires. Only suppress when fixing is impossible (framework constraint, Unity serialization, Harmony naming).

**How to suppress**: `.editorconfig` only — `dotnet_diagnostic.{ID}.severity = none` under the appropriate file glob, with a `# WHY:` comment explaining why the code cannot be fixed.

**Never use**: `[SuppressMessage]`, `#pragma warning disable` (except CS-prefixed compiler warnings), `<NoWarn>` in csproj for CA/IDE/RCS codes, `.ruleset` files.

**`[SuppressMessage]` exception**: Only when the suppression applies to a single specific member AND other code in the same glob should still trigger the rule AND no `.editorconfig` glob can isolate it. This is rare.

**NuGet warnings (NU prefix)**: Cannot use `.editorconfig`. Suppress in `Directory.Build.props` (category-wide) or individual `.csproj` (project-specific).
