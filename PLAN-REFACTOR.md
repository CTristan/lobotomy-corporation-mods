# Plan: RetargetHarmony Improvements

## Context

RetargetHarmony is a BepInEx 5 preloader patcher enabling Harmony 1.x/2.x coexistence. The core patching logic is solid and well-tested, but the codebase has accumulated duplicated config parsing, dead code, and test coverage gaps in the BaseMods integration layer.

## Goals

- Eliminate duplicated config discovery/parsing between `RetargetHarmony.cs` and `DebugLogger.cs`
- Remove dead code and fix minor code quality issues
- Add test coverage for currently untested methods (BaseMods integration)

## Approach

Three phases in order: config deduplication (largest structural change), dead code cleanup, then new tests. Each phase builds on the previous.

## Tasks

### Phase 1: Extract shared ConfigReader

**Files:** New `RetargetHarmony/ConfigReader.cs`, modify `RetargetHarmony.cs` + `DebugLogger.cs`

Currently both files independently discover and parse `RetargetHarmony.cfg` with duplicate path resolution (assembly dir → BepInEx config → fallback) and INI parsing. The log level gets set twice — once by DebugLogger's own parsing, once by RetargetHarmony calling `SetLogLevelFromConfig`.

- [ ] Create `RetargetHarmony/ConfigReader.cs` with:
  - `ConfigFileName` constant
  - `ConfigValues` class with `LogLevel` (string) and `PatchBaseMods` (bool) properties
  - `ResolveConfigPath(string assemblyDir)` — shared path resolution logic
  - `ParseConfigFile(string configPath)` → returns `ConfigValues`
  - `GenerateDefaultConfig(string configPath)` — writes default template
  - `ConfigDirectoryOverride` property (moved from DebugLogger, for testability)
  - Standardize parsing on `Split(new[] { '=' }, 2)` approach
  - Must NOT depend on DebugLogger (avoids circular init)
- [ ] Refactor `RetargetHarmony.InitializeConfig()` to use `ConfigReader` — remove `ParseConfigFile()` method (lines 219-268)
- [ ] Refactor `DebugLogger.Initialize()` to remove its own config parsing — remove `ParseConfigFile()` method (lines 343-398), have RetargetHarmony pass config values in
- [ ] Move `ConfigDirectoryOverride` from DebugLogger to ConfigReader
- [ ] Update `DebugLoggerTests.cs` references from `DebugLogger.ConfigDirectoryOverride` → `ConfigReader.ConfigDirectoryOverride`
- [ ] Create `RetargetHarmony.Test/ConfigReaderTests.cs` covering path resolution, parsing, default generation

### Phase 2: Dead code & minor cleanup

Each sub-task is independent.

- [ ] **Remove `WriteToUnityDebug` dead code** (DebugLogger.cs): Delete `s_unityAvailable` field, its reset in `Reset()`, the `shouldLogToUnity` logic in `Log()`, and the entire `WriteToUnityDebug` method
- [ ] **Simplify `GetAssemblyDirectory`** (DebugLogger.cs:400-418): Replace manual `/`+`\\` splitting with `Path.GetDirectoryName(location) ?? Environment.CurrentDirectory`
- [ ] **Remove unnecessary null check** (RetargetHarmony.cs:306): Change `if (harmonyRefs != null && harmonyRefs.Count > 0)` → `if (harmonyRefs.Count > 0)`
- [ ] **Consolidate Safe\* wrappers** (RetargetHarmony.cs:121-149): Replace 5 identical methods with a single helper. Note: `[CallerMemberName]` unavailable on .NET 3.5, so pass method name as string param. Trade-off: adds lambda allocation but only runs at startup, not a hot path
- [ ] **Remove unused imports** (DebugLogger.cs): `using BepInEx.Bootstrap;` and `using Mono.Cecil;`
- [ ] **Document volatility difference** (RetargetHarmony.cs): Add comment above `s_configInitialized`/`s_patchBaseMods` explaining they are not volatile because they are set during single-threaded initialization and then read-only

### Phase 3: Add missing tests

- [ ] Make `OnAssemblyResolve` public (entry point with no indirect test path; it's an event handler). Keep `TryResolveCecilAssembly` private — tested indirectly through `OnAssemblyResolve`
- [ ] Test `OnAssemblyResolve` (which exercises `TryResolveCecilAssembly` indirectly): returns null when directory doesn't exist, returns null when DLL not found, returns assembly when valid DLL found (copy real DLL to temp dir), returns null for invalid DLL (garbage bytes)
- [ ] Test `OnSaveAssemblyCache` via reflection: returns true by default, returns false when flag is disabled
- [ ] Add contract tests for `PreFindPluginTypes` and `PostFindPluginTypes` method signatures via reflection
- [ ] Document `AssemblyDefinition` lifetime in `TryResolveCecilAssembly` and `OnAssemblyResolve` — BepInEx owns the returned reference

## Risks & Considerations

- **Init ordering** (Phase 1): `DebugLogger.Initialize()` is called before `InitializeConfig()` in `Patch()`. The new ConfigReader must not depend on DebugLogger. Parse silently, return results, let callers log.
- **Double log-level setting**: Currently the log level is set by both DebugLogger's own parsing AND RetargetHarmony's call to `SetLogLevelFromConfig`. After Phase 1, it should be set exactly once.
- **Lambda allocation** (Phase 2, Safe\* consolidation): Negligible since this runs once at startup, but worth noting for a .NET 3.5 target.
- **Public visibility** (Phase 3): Only `OnAssemblyResolve` needs to be public (event handler with no indirect test path). `TryResolveCecilAssembly` stays private — tested indirectly through `OnAssemblyResolve`. Other private methods tested via reflection or contract tests.

## Verification

1. `dotnet build LobotomyCorporationMods.sln` — all projects compile
2. `dotnet test LobotomyCorporationMods.sln` — all existing + new tests pass
3. `dotnet ci` — formatting and warnings clean
