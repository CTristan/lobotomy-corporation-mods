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

## Constructor Parameter Rules

Every class falls into exactly one of two categories. No exceptions.

### Category 1: Fully Testable Classes

Constructor parameters are exclusively **primitives** or **interfaces**.

Allowed primitive types: `string`, `int`, `long`, `float`, `double`, `decimal`, `bool`, `char`, `byte`, enum types (for example `RwbpType`), `Type`.

Allowed reference types: any interface (for example `ILogger`, `IFileManager`, `ICreatureEvaluator`).

These classes:
- Contain business logic
- Must have 100% code coverage
- Must NOT have `[ExcludeFromCodeCoverage]` on the class

### Category 2: Boundary Wrapper Classes

Constructor parameters include at least one **concrete Unity or game type** (for example `AgentModel`, `CreatureModel`, `GameObject`, `Component` subclasses, `DirectoryInfo`).

These classes:
- Must have `[AdapterClass]` and `[ExcludeFromCodeCoverage]` on the class
- Must contain **minimal logic** — only delegation, property forwarding, or trivial mapping
- Exist solely to bridge concrete types behind interfaces
- Push all business logic to Category 1 classes that accept the resulting interfaces

### Guiding Principle

Minimize the surface area of boundary wrappers. When business logic lives in a class that takes concrete types, refactor by:
1. Creating an interface that exposes only the needed properties and methods
2. Creating a thin boundary wrapper (Category 2) that adapts the concrete type to that interface
3. Changing the business-logic class (Category 1) to accept the interface instead

## csproj Conventions

### Shared Properties Live in `Directory.Build.props`

The following properties are defined in `Directory.Build.props` and must NOT be duplicated in individual `.csproj` files:

- `AnalysisLevel`, `Deterministic`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`
- `TreatWarningsAsErrors`
- `OutputPath`
- `DebugType` (both Debug and Release configurations)
- `CheckForOverflowUnderflow`, `WarningLevel` (Release configuration)
- Roslynator.Analyzers and Roslynator.Formatting.Analyzers package references

If a project needs to **override** a shared property, it may do so in its own `.csproj` with a comment explaining why.

### Warning Suppression Rules (csproj)

#### Code analysis warnings (CA, IDE, RCS prefixes)

**Always** suppress in `.editorconfig` using `dotnet_diagnostic.{ID}.severity = none` under the appropriate file glob pattern. **Never** use:
- `<NoWarn>` in csproj for CA/IDE/RCS codes
- `<NoAnalyzer>` ItemGroups in csproj
- `.ruleset` files
- `[SuppressMessage]` attributes in source code

#### NuGet warnings (NU prefix)

These **cannot** be handled by `.editorconfig`. Suppress in `Directory.Build.props` if the suppression applies to a category of projects (e.g., all test projects), or in the individual `.csproj` if it's project-specific.

### csproj Format

- Use modern SDK-style format: `<Project Sdk="Microsoft.NET.Sdk">`
- No XML declaration (`<?xml ...?>`)
- No `xmlns` or `ToolsVersion` attributes on the `<Project>` element
- 2-space indentation (enforced by `.editorconfig`)
- No `.ruleset` file references (`<CodeAnalysisRuleSet>`)

### Nullable Reference Types

All projects that are **not** targeting `net35` must enable nullable reference types:

```xml
<Nullable>enable</Nullable>
```

net35 projects cannot use this feature (the compiler doesn't support it for that target).

### What Belongs in a `.csproj`

Only project-specific settings:
- `TargetFramework`
- `AssemblyVersion`, `Version`
- `OutputType` (if not Library)
- `IsPackable`, `PackAsTool`, `ToolCommandName`, `PackageId` (for tool projects)
- `ProjectGuid`
- `Nullable` (required for all non-net35 projects)
- Project-specific `<NoWarn>` for NU-prefixed warnings only
- Package references unique to the project
- Project references
- External DLL references
- Content/resource includes

## Interface And Implementation Conventions

### Folder And Namespace Layout

- Place interfaces under an `Interfaces/` folder.
- Place concrete classes under an `Implementations/` folder.
- Match namespaces to folder segments:
  - `...Interfaces`
  - `...Implementations`
  - nested folders become nested namespaces (for example `Interfaces.Adapters.BaseClasses`, `Implementations.CreatureEvaluators`).

### Naming

- Interface names must use the `I` prefix (for example `ILogger`, `IAgentWorkTracker`, `ICreatureEvaluator`).
- Implementation names should use the same root name without `I` (for example `Logger : ILogger`, `AgentWorkTracker : IAgentWorkTracker`).
- Adapter interfaces and implementations should keep explicit domain names (for example `ITextTestAdapter` and `TextTestAdapter`).

### Visibility

- Use `public` for shared contracts and implementations in `LobotomyCorporationMods.Common`.
- Use `internal` for mod-private contracts and implementations that should not leak outside a specific mod assembly.
- Prefer `sealed` for concrete leaf implementations.
- Keep non-sealed classes only when they are intentional base classes (for example generic adapter base classes or abstract evaluator bases).
- **Never** use `InternalsVisibleTo`. Internal members must either be `public` themselves or be exercised indirectly through a public method. If neither is possible, restructure so testable logic is reachable from a public surface.

### Inheritance And Composition Patterns

- Use interfaces for seams that are injected, mocked, or swapped at runtime.
- For related families, define a small interface plus optional abstract base class:
  - example pattern: `ICreatureEvaluator` + `CreatureEvaluator` + sealed evaluator types.
- For adapter hierarchies, use generic base interfaces and base classes to share behavior:
  - example pattern: `ITestAdapter<T>` -> `IComponentTestAdapter<T>` and `TestAdapter<T>` -> `ComponentTestAdapter<T>`.
- Keep parameter/data-holder classes as plain concrete types unless a real polymorphic need exists.

### Dependency Usage

- Prefer interface-typed fields/properties/parameters at composition boundaries.
- You may internally instantiate a concrete class only if that class is itself Category 1 (fully testable — primitives and interfaces only in its constructor). Never internally instantiate a Category 2 boundary wrapper; those must always enter via an interface parameter.
- In patch methods and extension methods, accept interface parameters for collaborators that may be substituted in tests.

### Exceptions You Can Follow Intentionally

- A concrete class may implement an interface directly instead of inheriting the base class when acting as a sentinel or no-op implementation (for example `NoneEvaluator : ICreatureEvaluator`).
- Intermediate base classes are allowed to be non-sealed when they exist only to support inheritance for concrete sealed leaf types.

## JsonUtility Serialization Rules

Unity's `JsonUtility` cannot serialize dictionaries, anonymous objects, or properties. All JSON-serializable types must follow these rules.

### Folder Convention

All JsonUtility data classes **must** live in a `JsonModels/` folder within their project. This is mandatory — `.editorconfig` suppresses CA1051, IDE1006, and CA1708 globally via the `[**/JsonModels/**.cs]` glob, so placing classes elsewhere means they won't get the correct suppressions.

```
MyProject/
  JsonModels/          # All [Serializable] data classes go here
    GameStateData.cs
    ScreenshotData.cs
  OtherCode/           # Non-serialization code stays outside
```

Do **not** place non-JsonUtility classes in `JsonModels/` — the relaxed analyzer rules only make sense for serialization data classes.

### Data Classes

Every class serialized by `JsonUtility` must:

1. Live in the project's `JsonModels/` folder
2. Be marked `[Serializable]`
3. Use **public lowercase fields only** — no properties
4. Use only supported types: `string`, `int`, `float`, `double`, `bool`, `long`, `byte`, enum types, arrays of serializable types, or nested `[Serializable]` classes

Do **not** use `[SuppressMessage]` for CA1051, IDE1006, or CA1708 — the `JsonModels/` folder convention handles this via `.editorconfig`.

```csharp
[Serializable]
public class GameStateData
{
    public int day;
    public string gameState;
    public float energy;
    public bool isPaused;
}
```

#### Do NOT

- Add PascalCase property accessors that shadow lowercase fields — this causes serialization failures
- Use `Dictionary<K, V>` on types serialized outbound (only works for inbound deserialization)
- Use anonymous objects or LINQ projections as serialization targets
- Use `List<T>` — prefer arrays (`T[]`) for JsonUtility compatibility

### Placeholder Serialization Pattern

When a response must carry an arbitrary typed object (polymorphic data), use the placeholder trick from `MessageSerializer.cs`:

1. Declare a `string` field on the wrapper to hold a placeholder value
2. Store the actual object in a `[NonSerialized] object` field
3. Serialize the wrapper with a placeholder string, serialize the data object separately, then string-replace the placeholder with the real JSON

```csharp
// In the wrapper class
public string data; // Placeholder field for JsonUtility
[NonSerialized]
public object DataObject; // Actual data, not serialized directly

// In the serializer
response.data = "__DATA_PLACEHOLDER__";
string wrapperJson = JsonUtility.ToJson(response);
string dataJson = JsonUtility.ToJson(response.DataObject);
return wrapperJson.Replace("\"__DATA_PLACEHOLDER__\"", dataJson);
```

Reference implementations: `Protocol/Response.cs`, `Protocol/MessageSerializer.cs`.

## Test Conventions

### Structure

- **Test files must never be placed directly in the test project root** — every test file must be in at least one subfolder
- Mirroring the source project's folder structure is preferred but not required — use a more logical grouping when it improves clarity (e.g., shared helpers in a `Helpers/` folder, or grouping related tests differently than their source layout)
- Mod tests live in `ModTests/{ModName}Tests/` mirroring the mod's structure
- Each mod has a base class `{ModName}ModTests` with shared setup (instantiates `Harmony_Patch`, configures mock logger)
- Patch tests go in `PatchTests/{ClassName}Patch{MethodName}Tests.cs`
- `HarmonyPatchTests.cs` at the mod level verifies all patch registrations and exception logging

### Test Naming

Test methods are descriptive sentences with underscores:

```csharp
A_gift_that_has_not_been_worked_on_yet_displays_the_base_value
Does_not_attempt_to_update_newly_generated_agents
Changing_random_generated_agent_marks_them_as_custom
```

### Assertions

Use FluentAssertions — never raw `Assert`:

```csharp
result.Should().Be(expected);
result.Should().BeEquivalentTo(expected);
collection.Should().ContainValue(expected);
action.Should().Throw<ArgumentNullException>();
```

### Mocking

Use Moq for interfaces. Common patterns:

```csharp
var mockLogger = TestExtensions.GetMockLogger();
mockLogger.VerifyArgumentNullException(Action);
mockLogger.VerifyNullReferenceException(Action);
```

### Unity test objects

Use `UnityTestExtensions` factory methods — never `new` Unity types directly:

```csharp
UnityTestExtensions.CreateAgentModel()
UnityTestExtensions.CreateCreatureEquipmentMakeInfo()
UnityTestExtensions.CreateCustomizingWindow(currentAgent, currentWindowType)
UnityTestExtensions.CreateWorkerSpriteManager()
```

### Every patch must have

1. **Registration test** — `ValidateHarmonyPatch(originalClass, methodName)` in `HarmonyPatchTests.cs`
2. **Exception logging test** — call Postfix/Prefix with null, verify via `mockLogger.VerifyArgumentNullException`
3. **Business logic tests** — call the extension method directly (not the Postfix/Prefix)

### Test Pattern

```csharp
[Fact]
public void Descriptive_test_name()
{
    // Arrange
    var sut = InitializeGameObject();

    // Act
    var result = sut.PatchAfterMethod(args);

    // Assert
    result.Should().Be(expected);
}
```

### Coverage

100% patch coverage is enforced via Codecov. The `[ExcludeFromCodeCoverage]` Postfix/Prefix methods are excluded — test the extension methods instead.

## No Direct Unity Engine Calls in Testable Code

Unity types rely on native internal calls (`ECall`) that throw `System.Security.SecurityException` outside the Unity runtime — including in xUnit test hosts. This applies to:

- **Unity static methods**: `Debug.Log`, `Debug.LogError`, `Object.Instantiate`, etc.
- **Concrete game types that inherit from Unity types**: `AgentModel`, `CreatureModel`, `GameObject`, `Component` subclasses, and any other class rooted in `UnityEngine.Object`

Constructing, accessing, or calling methods on these types in test context will crash the test host with the same `ECall` error — even if your code never calls a `UnityEngine.*` method directly.

### Rule

**Never use Unity types or game types that inherit from Unity types in Category 1 (fully testable) classes.** Access them only through interfaces.

Direct Unity type usage belongs only in Category 2 boundary wrappers marked `[ExcludeFromCodeCoverage]`. See the Constructor Parameter Rules section for the Category 1/2 framework.

### Wrong

```csharp
// Category 1 class — this will crash in tests
public class ClientHandler
{
    public void HandleError(Exception ex)
    {
        UnityEngine.Debug.LogError($"Error: {ex.Message}");
    }
}
```

### Right

```csharp
// Category 1 class — testable, uses injected interface
public class ClientHandler
{
    private readonly ILogger _logger;

    public ClientHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void HandleError(Exception ex)
    {
        _logger.WriteException(ex);
    }
}
```

The project provides `ILogger` and `ILoggerTarget` in `LobotomyCorporationMods.Common` for this purpose.

## Warning Suppression Rules

### First Priority: Fix the Warning

**Analyzer warnings exist to improve code quality.** When you encounter a warning, your **first and strongest obligation** is to fix the code so the warning no longer fires. Suppression of any kind is a **last resort**, not a convenience.

Before suppressing a warning, you must:

1. **Understand the rule** — read what the analyzer is flagging and why
2. **Fix the code** — rename, refactor, restructure, or redesign to satisfy the rule
3. **Only if the fix is impossible** (e.g., a framework constraint, Unity serialization requirement, or Harmony naming convention) should you suppress

If you suppress a warning, you must explain **why fixing the code is not possible** — not just what the warning is.

### When Suppression Is Necessary

`.editorconfig` is the **single source of truth** for analyzer rule exclusions. Never use `[SuppressMessage]`, `#pragma warning disable`, or `<NoWarn>` for analyzer diagnostics.

Add a `dotnet_diagnostic.{ID}.severity = none` entry under the appropriate file glob pattern in `.editorconfig`, with a `# WHY:` comment explaining **why the code cannot be fixed** to satisfy the rule.

#### Correct — `.editorconfig` with scoped glob (when fix is impossible)

```ini
# WHY: JsonUtility data classes require public lowercase fields and case-differing accessors — Unity's
# serializer only works with public fields, and JSON keys must match the external schema
[**/JsonModels/**.cs]
dotnet_diagnostic.CA1051.severity = none
dotnet_diagnostic.IDE1006.severity = none
dotnet_diagnostic.CA1708.severity = none
```

The `JsonModels/` folder convention ensures all JsonUtility data classes are automatically covered (see the JsonUtility Serialization Rules section).

#### Wrong — `[SuppressMessage]` on individual classes

```csharp
// DO NOT do this
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public class GameStateData { ... }
```

#### Wrong — `#pragma warning disable` for analyzer rules

```csharp
// DO NOT do this
#pragma warning disable CA1051
public string myField;
#pragma warning restore CA1051
```

### When `[SuppressMessage]` Is Acceptable

Only when **all** of the following are true:

1. The suppression applies to a **single specific member** (not a whole file or folder)
2. Other code in the same file glob **should** still trigger the rule
3. No `.editorconfig` glob can reasonably isolate the case

This is rare. If you find yourself reaching for `[SuppressMessage]`, first check whether a narrower `.editorconfig` glob (e.g., `[**/Protocol/**.cs]`) can scope the exclusion instead.

### When `#pragma warning disable` Is Acceptable

Only for **compiler warnings** (CS-prefixed codes) that `.editorconfig` cannot suppress. Analyzer diagnostics (CA, IDE, RCS) must always go in `.editorconfig`.

### Complementary Rules

- **csproj files**: Never use `<NoWarn>` for CA/IDE/RCS codes — see the csproj Conventions section
- **`.editorconfig` comments**: Every `dotnet_diagnostic` entry must have a `# WHY:` comment explaining why the code **cannot be fixed**, not just restating the rule name
