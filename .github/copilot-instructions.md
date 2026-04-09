# Instructions

This file provides guidance when working with code in this repository.

## Project Overview

Harmony mods for the Unity game Lobotomy Corporation. Each mod is an independent project that patches game methods at runtime via Harmony. Mods are loaded by Basemod/Lobotomy Mod Manager (LMM), not BepInEx.

## Build and Test Commands

```bash
dotnet build LobotomyCorporationMods.sln
dotnet test LobotomyCorporationMods.sln
dotnet test /p:CollectCoverage=true /p:CoverletOutput="./coverage.opencover.xml" /p:CoverletOutputFormat=opencover LobotomyCorporationMods.sln
```

Run a single test:
```bash
dotnet test --filter "FullyQualifiedName~ClassName.MethodName" LobotomyCorporationMods.sln
```

Game DLLs in `external/LobotomyCorp_Data/Managed/` are required but not committed. CI pulls them from a private repo.

## Target Frameworks

- **Mod projects:** `net35` (game uses old Unity/.NET 3.5)
- **Test project:** `net481`
- **Tool projects (CI, SetupExternal):** `net10.0`

`TreatWarningsAsErrors` is enabled on all projects.

## Architecture

### Mod Structure

Every mod project contains:
- `Harmony_Patch.cs` - Required entry point class (Basemod convention). Extends `HarmonyPatchBase` from Common. Singleton pattern with `Instance` field.
- `Patches/` - One class per patched game method, named `{TargetClass}Patch{MethodName}`

### Two-Method Patch Pattern

Each patch class has exactly two methods:
1. **Extension method** (`PatchAfter{MethodName}`) - Contains all business logic. Fully testable.
2. **Postfix method** - Marked `[EntryPoint]` and `[ExcludeFromCodeCoverage]`. Delegates to the extension method, wraps in try/catch logging. Uses Harmony magic parameters (`__instance`, `__result`).

Patches must be **Postfix** unless Prefix is unavoidable (requires a comment explaining why).

### Common Project (`LobotomyCorporationMods.Common`)

Shared infrastructure: `HarmonyPatchBase`, `FileManager`, `Logger`, extension methods for game types, attributes (`EntryPoint`, `ExcludeFromCodeCoverage`), and test adapters for Unity components.

### Test Project (`LobotomyCorporationMods.Test`)

Single test project covering all mods. Organized as `ModTests/{ModName}Tests/` with:
- `{ModName}ModTests.cs` - Base class with shared test utilities
- `HarmonyPatchTests.cs` - Validates patch attributes and exception handling
- `PatchTests/` - Tests for individual patch extension methods

**Stack:** xUnit, FluentAssertions, Moq. Test names use descriptive sentences with underscores. 100% coverage target for business logic.

## Coding Conventions

- Never use `InternalsVisibleTo` - make code public or test indirectly through public methods
- Use `ThrowHelper.ThrowIfNull` instead of `Guard.Against.Null`
- File header: `// SPDX-License-Identifier: MIT`
- Instance fields: `_camelCase`; static fields: `s_camelCase`; constants: `PascalCase`
- `.editorconfig` enforces all formatting rules

## Documentation Checklist (when updating a mod)

Update: `.csproj` version, `CHANGELOG.md`, `INTEGRATION_TESTING_CHECKLIST.md`, root `README.md`, mod `README.md`, and `Info/Info.xml`.
