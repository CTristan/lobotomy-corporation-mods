# Plan: Standardize csproj Files and Consolidate Warning Suppressions

## Context

The 22 `.csproj` files in this repo have accumulated inconsistencies over time:
- **Mixed XML formats**: Mod projects use verbose XML (declaration, xmlns, ToolsVersion, 2-space indent) while tool projects use modern SDK-style (4-space indent, no declaration)
- **Warning suppressions scattered across 3 mechanisms**: `<NoWarn>` properties, `<NoAnalyzer>` ItemGroups, and a `.ruleset` file — many of which should live in `.editorconfig`
- **Duplicated build properties**: Every csproj repeats AnalysisLevel, Deterministic, EnableNETAnalyzers, EnforceCodeStyleInBuild, TreatWarningsAsErrors, Roslynator packages, and Debug/Release configurations
- **LobotomyPlaywright.Plugin.ruleset**: A 345-rule suppression file that no other net35 project needs

The goal is to centralize shared settings, move code analysis suppressions to `.editorconfig`, and standardize all csproj files to the modern SDK-style format.

## Goals

- All csproj files use consistent modern SDK-style format (no XML declaration, no xmlns/ToolsVersion, 2-space indent)
- Shared build properties live in `Directory.Build.props`
- All CA/IDE/RCS warning suppressions are in `.editorconfig`, not in csproj files
- Only NU-prefixed (NuGet) warnings remain in `Directory.Build.props` since `.editorconfig` cannot handle them
- The `.ruleset` file is deleted; any truly needed suppressions will be added to `.editorconfig` later if builds fail
- Each csproj contains only what's unique to that project

## Completed Pre-Tasks

- [x] Created `.github/instructions/csproj-conventions.instructions.md` — enforces where build properties and warning suppressions belong
- [x] Updated `.github/prompts/new-mod.prompt.md` — references Directory.Build.props and new csproj-conventions instruction
- [x] Created `CSPROJ-FINDINGS.md` — detailed audit of all 22 csproj files

## Tasks

### 1. Create `Directory.Build.props`

Create `Directory.Build.props` in the repo root with properties shared by ALL projects:

```xml
<Project>
  <PropertyGroup>
    <AnalysisLevel>latest-all</AnalysisLevel>
    <Deterministic>true</Deterministic>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <OutputPath>bin\</OutputPath>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)' == 'Debug' ">
    <DebugType>full</DebugType>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
    <CheckForOverflowUnderflow>true</CheckForOverflowUnderflow>
    <DebugType>none</DebugType>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Roslynator.Analyzers" Version="4.15.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Roslynator.Formatting.Analyzers" Version="4.15.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  <!-- NU1702: net35 project references intentionally loaded via modern .NET compatibility -->
  <PropertyGroup Condition="'$(IsTestProject)' == 'true'">
    <NoWarn>$(NoWarn);NU1702</NoWarn>
  </PropertyGroup>
</Project>
```

**Notes:**
- `WarningLevel=4` and `CheckForOverflowUnderflow` are in Release only (currently some non-test projects lack the Release block entirely — this standardizes it)
- `TreatWarningsAsErrors` only needs to appear once (currently duplicated in both main and Release PropertyGroups)
- The NU1702 conditional applies only when `IsTestProject=true`, covering all test projects automatically
- NU1605 stays project-specific in `RetargetHarmony.Test.csproj` (unique to that project's Mono.Cecil version conflict)

### 2. Update `.editorconfig` with new sections

Add sections for projects that currently suppress warnings via csproj/NoAnalyzer. File: `.editorconfig`

**New sections to add:**

```ini
# WHY: CI is a CLI tool with the same conventions as SetupExternal
[CI/**.cs]
dotnet_diagnostic.CA1303.severity = none
dotnet_diagnostic.CA1515.severity = none
dotnet_diagnostic.CA1852.severity = none
dotnet_diagnostic.CA1034.severity = none
dotnet_diagnostic.CA1002.severity = none
dotnet_diagnostic.CA1031.severity = none
dotnet_diagnostic.CA1812.severity = none

# WHY: LobotomyPlaywright CLI tool shares conventions with CI/SetupExternal
[LobotomyPlaywright/**.cs]
dotnet_diagnostic.CA1303.severity = none
dotnet_diagnostic.CA1515.severity = none
dotnet_diagnostic.CA1852.severity = none
dotnet_diagnostic.CA1034.severity = none
dotnet_diagnostic.CA1002.severity = none
dotnet_diagnostic.CA1031.severity = none
dotnet_diagnostic.CA1812.severity = none
dotnet_diagnostic.CA1869.severity = none
dotnet_diagnostic.CA1305.severity = none
dotnet_diagnostic.CA1304.severity = none
dotnet_diagnostic.CA1311.severity = none
dotnet_diagnostic.CA1310.severity = none
dotnet_diagnostic.CA1308.severity = none
dotnet_diagnostic.CA1861.severity = none
dotnet_diagnostic.CA1822.severity = none
dotnet_diagnostic.RCS1075.severity = none
```

**Update existing `[**.Test/**.cs]` section** to also suppress:
- `CA1303` (localization — test projects use string literals)
- `CA1812` (unused private types — test helper classes)
- `IDE0051` (unused private members — test helper methods)

**Note on LobotomyPlaywright.Plugin:** The `.ruleset` file is being deleted. If new build warnings appear, we'll add targeted suppressions to `.editorconfig` under a `[LobotomyPlaywright.Plugin/**.cs]` section. The other net35 mod projects don't need these suppressions.

### 3. Standardize each csproj file

For **every** csproj file:
- Remove `<?xml version="1.0" encoding="utf-8"?>` declaration
- Remove `xmlns="http://schemas.microsoft.com/developer/msbuild/2003"` and `ToolsVersion="4.0"` from `<Project>` tag
- Standardize to 2-space indentation (matching `.editorconfig` rule for csproj)
- Remove properties now in `Directory.Build.props`: AnalysisLevel, Deterministic, EnableNETAnalyzers, EnforceCodeStyleInBuild, TreatWarningsAsErrors (both main and Release), DebugType (both Debug and Release), OutputPath, CheckForOverflowUnderflow, WarningLevel
- Remove empty Debug/Release PropertyGroups if they become empty after removing shared properties
- Remove `<NoWarn>` entries for CA/IDE/RCS codes (keep only NU-prefixed if project-specific)
- Remove all `<NoAnalyzer>` ItemGroups
- Remove `<CodeAnalysisRuleSet>` references
- Remove `<RunAnalyzersDuringBuild>` and `<RunAnalyzers>` if they're just set to `true` (default)
- Remove Roslynator PackageReference entries (now in Directory.Build.props)
- Remove duplicate `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` from Release PropertyGroup

**Files to modify (22 total):**

Mod projects (net35) — remove verbose XML, shared properties, Roslynator refs:
- `LobotomyCorporationMods.Common/LobotomyCorporationMods.Common.csproj`
- `LobotomyCorporationMods.BadLuckProtectionForGifts/LobotomyCorporationMods.BadLuckProtectionForGifts.csproj`
- `LobotomyCorporationMods.BugFixes/LobotomyCorporationMods.BugFixes.csproj`
- `LobotomyCorporationMods.FreeCustomization/LobotomyCorporationMods.FreeCustomization.csproj`
- `LobotomyCorporationMods.GiftAlertIcon/LobotomyCorporationMods.GiftAlertIcon.csproj`
- `LobotomyCorporationMods.NotifyWhenAgentReceivesGift/LobotomyCorporationMods.NotifyWhenAgentReceivesGift.csproj`
- `LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking/LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.csproj`

BepInEx projects (net35) — remove shared properties, Roslynator refs, ruleset ref:
- `HarmonyDebugPanel/HarmonyDebugPanel.csproj`
- `RetargetHarmony/RetargetHarmony.csproj`
- `LobotomyPlaywright.Plugin/LobotomyPlaywright.Plugin.csproj` — also remove `CodeAnalysisRuleSet`

Tool projects (net10.0) — remove shared properties, NoAnalyzer, fix indent:
- `CI/CI.csproj`
- `SetupExternal/SetupExternal.csproj`
- `LobotomyPlaywright/LobotomyPlaywright.csproj` — also remove massive `<NoWarn>` line

Test projects (net10.0) — remove shared properties, NoAnalyzer, NoWarn(CA/IDE):
- `LobotomyCorporationMods.Test/LobotomyCorporationMods.Test.csproj`
- `CI.Test/CI.Test.csproj`
- `SetupExternal.Test/SetupExternal.Test.csproj`
- `HarmonyDebugPanel.Test/HarmonyDebugPanel.Test.csproj`
- `RetargetHarmony.Test/RetargetHarmony.Test.csproj`
- `LobotomyPlaywright.Test/LobotomyPlaywright.Test.csproj`
- `LobotomyPlaywright.Plugin.Test/LobotomyPlaywright.Plugin.Test.csproj`

### 4. Delete the ruleset file

- Delete `LobotomyPlaywright.Plugin/LobotomyPlaywright.Plugin.ruleset`

### 5. Standardize package versions

While touching every csproj, fix version inconsistencies:
- `LobotomyPlaywright.Plugin.Test` uses older `Microsoft.NET.Test.Sdk 17.8.0` → update to `17.10.0`
- `LobotomyPlaywright.Plugin.Test` uses older `xunit 2.6.6` / `xunit.runner.visualstudio 2.5.6` → update to `2.8.1`
- `LobotomyPlaywright.Plugin.Test` is missing `xunit.runner.console` — add it for consistency with other test projects

### 6. Build, test, and fix

- Run `dotnet build LobotomyCorporationMods.sln` — fix any new warnings (especially from removing the .ruleset)
- Run `dotnet test /p:CollectCoverage=true /p:CoverletOutput="./coverage.opencover.xml" /p:CoverletOutputFormat=opencover LobotomyCorporationMods.sln`
- Run `dotnet ci --check` to verify formatting

## Risks & Considerations

- **Removing the .ruleset may reveal warnings in LobotomyPlaywright.Plugin**: This is intentional — we'll add only the suppressions that are actually needed to `.editorconfig`. The other net35 projects compile clean without a ruleset, so most of these suppressions are likely unnecessary.
- **Directory.Build.props applies to ALL projects**: MSBuild auto-discovers this file. Verify that no project breaks from inheriting the shared properties (especially `OutputPath=bin\` which wasn't set on LobotomyPlaywright.csproj before).
- **Modern SDK-style format for net35 mods**: The `xmlns` and `ToolsVersion` attributes are vestigial and not needed by modern MSBuild. Removing them is safe.
- **NU1702 conditional on IsTestProject**: Non-test projects that reference net35 assemblies directly (via `<Reference>`) don't get NU1702 — only `<ProjectReference>` from net10.0→net35 triggers it, which only happens in test projects.

## Verification

1. `dotnet build LobotomyCorporationMods.sln` — zero errors, zero warnings
2. `dotnet test /p:CollectCoverage=true /p:CoverletOutput="./coverage.opencover.xml" /p:CoverletOutputFormat=opencover LobotomyCorporationMods.sln` — all tests pass
3. `dotnet ci --check` — formatting passes
4. Spot-check that no csproj contains `<NoWarn>` with CA/IDE/RCS codes, `<NoAnalyzer>`, `<CodeAnalysisRuleSet>`, or `.ruleset` references
5. Verify `.editorconfig` has appropriate sections for CI, LobotomyPlaywright, and test projects
