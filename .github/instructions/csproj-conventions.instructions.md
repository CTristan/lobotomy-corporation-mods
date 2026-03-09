---
description: "Use when creating or modifying .csproj files, adding NuGet packages, or suppressing build warnings. Enforces where build properties and warning suppressions belong."
applyTo: "**/*.csproj"
---

# csproj Conventions

## Shared Properties Live in `Directory.Build.props`

The following properties are defined in `Directory.Build.props` and must NOT be duplicated in individual `.csproj` files:

- `AnalysisLevel`, `Deterministic`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`
- `TreatWarningsAsErrors`
- `OutputPath`
- `DebugType` (both Debug and Release configurations)
- `CheckForOverflowUnderflow`, `WarningLevel` (Release configuration)
- Roslynator.Analyzers and Roslynator.Formatting.Analyzers package references

If a project needs to **override** a shared property, it may do so in its own `.csproj` with a comment explaining why.

## Warning Suppression Rules

### Code analysis warnings (CA, IDE, RCS prefixes)

**Always** suppress in `.editorconfig` using `dotnet_diagnostic.{ID}.severity = none` under the appropriate file glob pattern. **Never** use:
- `<NoWarn>` in csproj for CA/IDE/RCS codes
- `<NoAnalyzer>` ItemGroups in csproj
- `.ruleset` files

### NuGet warnings (NU prefix)

These **cannot** be handled by `.editorconfig`. Suppress in `Directory.Build.props` if the suppression applies to a category of projects (e.g., all test projects), or in the individual `.csproj` if it's project-specific.

## Format

- Use modern SDK-style format: `<Project Sdk="Microsoft.NET.Sdk">`
- No XML declaration (`<?xml ...?>`)
- No `xmlns` or `ToolsVersion` attributes on the `<Project>` element
- 2-space indentation (enforced by `.editorconfig`)
- No `.ruleset` file references (`<CodeAnalysisRuleSet>`)

## Nullable Reference Types

All projects that are **not** targeting `net35` must enable nullable reference types:

```xml
<Nullable>enable</Nullable>
```

net35 projects cannot use this feature (the compiler doesn't support it for that target).

## What Belongs in a `.csproj`

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
