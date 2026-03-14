# Plan: Unified Debug Panel Mod

## Context

The existing `HarmonyDebugPanel` is a BepInEx 5 plugin that provides an in-game
IMGUI overlay and diagnostic logging for Harmony patch inspection. It works well
but **requires BepInEx to be installed**, which not all users have. Many users
run mods through LMM/Basemod with Harmony 1.09 only.

This plan creates a new mod, `LobotomyCorporationMods.DebugPanel`, that:

1. **Always works** as a standard LMM/Basemod mod using Harmony 1.09
2. **Dynamically enhances** itself when BepInEx and Harmony 2 are detected at
   runtime, using their APIs as the single source of truth for patch data
3. **Eventually replaces** `HarmonyDebugPanel` once proven reliable

### Existing infrastructure to reuse

- **HarmonyDebugPanel** (`HarmonyDebugPanel/`) — Reference implementation for
  collectors, overlay rendering, report building, and configuration
- **Common logging** (`LobotomyCorporationMods.Common`) — `ILogger`,
  `ILoggerTarget`, `FileLoggerTarget`, `IFileManager` interfaces
- **Mod pattern** — `Harmony_Patch.cs` singleton, two-method patch design,
  Category 1/2 class split

### Key Harmony 1.09 internals

Harmony 1.09 stores patch state in `HarmonySharedState`, a static class with an
internal dictionary mapping `MethodBase` to patch info. This is accessible via
reflection and provides active patch data without Harmony 2.

### Key Harmony 2 advantage

`HarmonyLib.Harmony.GetAllPatchedMethods()` returns patches from **all** Harmony
instances, including 1.09 patches. When available, this is the preferred single
source of truth for all active patch data.

## Goals

- [ ] In-game IMGUI diagnostic overlay that works with LMM/Basemod + Harmony
  1.09 alone (no BepInEx required)
- [ ] Dynamic detection of BepInEx/Harmony 2 at runtime, upgrading data
  collection when available
- [ ] When BepInEx is present, use Harmony 2 APIs for all patch data (both
  Harmony 1 and 2 patches)
- [ ] Feature parity with current HarmonyDebugPanel (overlay, hotkeys, config,
  log generation, expected vs actual patch comparison)
- [ ] JSON config file for hotkeys and section visibility (no BepInEx
  `ConfigFile` dependency)
- [ ] Full test coverage on business logic using Category 1/2 split with mocked
  interfaces for reflection boundaries
- [ ] Once proven, retire `HarmonyDebugPanel` in favor of this mod

## Approach

### Loading mechanism

The mod loads as a standard LMM/Basemod mod via `Harmony_Patch.cs`. During
initialization, it patches a game startup method (e.g., `GameManager.Awake` or
similar early-lifecycle method) to inject a `GameObject` with a custom
`MonoBehaviour`. This gives us `OnGUI()` for IMGUI rendering and `Update()` for
hotkey input — both are built into `UnityEngine.dll`, not BepInEx.

### Dynamic detection strategy

At startup, the mod checks `AppDomain.CurrentDomain.GetAssemblies()` for:

1. **Harmony 2**: Assembly named `0Harmony` containing `HarmonyLib.Harmony` type
2. **BepInEx**: Assembly named `BepInEx` containing
   `BepInEx.Bootstrap.Chainloader` type

Detection results determine which collector implementations are used:

| Component | Standalone (Harmony 1.09) | Enhanced (BepInEx + Harmony 2) |
|-----------|--------------------------|-------------------------------|
| Active patches | `HarmonySharedState` reflection | `Harmony.GetAllPatchedMethods()` via reflection |
| Mod listing | Filesystem scan of `BaseMods/` | Filesystem scan + BepInEx plugin listing |
| Patcher status | Not available | RetargetHarmony detection |
| Patch data | Harmony 1.09 patches only | All patches (Harmony 1 + 2) |
| Assemblies | `AppDomain.GetAssemblies()` | Same |
| Expected patches | Reflection + source scanning | Same |
| Version classification | Assembly reference inspection | Same |

### Category 1/2 split

All reflection-based access to Harmony internals, BepInEx APIs, and Unity
runtime objects goes into thin **Category 2** wrapper classes (marked
`[AdapterClass]` + `[ExcludeFromCodeCoverage]`). These implement interfaces that
**Category 1** business logic classes consume. This keeps all data processing,
comparison logic, and report building fully testable with mocks.

### Configuration

A JSON config file in the mod directory (e.g., `DebugPanel.config.json`), read
via `IFileManager` + `JsonUtility`. Fields:

- `overlayToggleKey` (string, default `"F9"`)
- `refreshKey` (string, default `"F10"`)
- `showActivePatches` (bool, default `true`)
- `showExpectedPatches` (bool, default `true`)
- `showAssemblyInfo` (bool, default `true`)
- `showBepInExPlugins` (bool, default `true`)
- `showLmmMods` (bool, default `true`)

## Tasks

### Phase 1: Project scaffolding and configuration

- [ ] Create `LobotomyCorporationMods.DebugPanel/` project (net35, SDK-style
  csproj)
  - References: `UnityEngine.dll`, `0Harmony109.dll`,
    `LobotomyCorporationMods.Common`
  - `<Private>false</Private>` on all external game DLL references
- [ ] Create `Harmony_Patch.cs` following the standard singleton pattern
  extending `HarmonyPatchBase`
- [ ] Define config JSON model class in `JsonModels/DebugPanelConfig.cs`
  - `[Serializable]`, public fields, PascalCase property accessors
  - All supported config fields with sensible defaults
- [ ] Create `IConfigProvider` interface and `ConfigProvider` implementation
  - Category 2 wrapper for file I/O, reads/writes JSON config
  - Falls back to defaults if config file doesn't exist
- [ ] Add test folder `LobotomyCorporationMods.Test/ModTests/DebugPanelTests/`

### Phase 2: Runtime detection and collector interfaces

- [ ] Create `IEnvironmentDetector` interface
  - `bool IsHarmony2Available { get; }`
  - `bool IsBepInExAvailable { get; }`
- [ ] Create `EnvironmentDetector` (Category 2) — scans
  `AppDomain.CurrentDomain.GetAssemblies()` for known assembly names
- [ ] Create `IActivePatchCollector` interface
  - `IReadOnlyList<PatchInfo> GetActivePatches()`
- [ ] Create `Harmony1PatchCollector` (Category 2) — reflects into
  `HarmonySharedState` to extract active patch data from Harmony 1.09
  - Target: `Harmony.HarmonySharedState` static class
  - Access internal dictionary mapping `MethodBase` to patch info
  - Extract: target method, patch method, owner ID, patch type
    (prefix/postfix/transpiler)
- [ ] Create `Harmony2PatchCollector` (Category 2) — reflects into
  `HarmonyLib.Harmony.GetAllPatchedMethods()` and `Harmony.GetPatchInfo()`
  - This is the preferred source when available — covers both Harmony 1 and 2
    patches
- [ ] Create `IExpectedPatchSource` interface (port from HarmonyDebugPanel)
  - Three-phase scan: reflection for `[HarmonyPatch]` attributes, source file
    regex fallback, runtime fallback
- [ ] Create `ExpectedPatchSource` implementation
  - Phase 1 (reflection) and Phase 2 (source scanning) are Category 1
  - Phase 3 (runtime query) delegates to `IActivePatchCollector`
- [ ] Create `IAssemblyInfoCollector` interface and implementation
  - Lists loaded assemblies, highlights Harmony-related ones
- [ ] Create `IHarmonyVersionClassifier` interface and implementation
  - Inspects assembly references to classify Harmony 1 vs 2 usage
- [ ] Create `IBaseModCollector` interface and implementation
  - Filesystem scan of `BaseMods/` directory for LMM mods
- [ ] Create `IBepInExPluginCollector` interface
  - Implementation reflects into `BepInEx.Bootstrap.Chainloader.PluginInfos`
  - Returns empty list when BepInEx is not available
- [ ] Create `IRetargetHarmonyDetector` interface
  - Implementation checks for RetargetHarmony patcher assembly
  - Returns "not available" when BepInEx is not present
- [ ] Create collector factory/strategy that selects implementations based on
  `IEnvironmentDetector` results
  - When Harmony 2 available: use `Harmony2PatchCollector` for all active
    patches
  - When Harmony 1.09 only: use `Harmony1PatchCollector`
  - When BepInEx available: enable `IBepInExPluginCollector` and
    `IRetargetHarmonyDetector`

### Phase 3: Data models and report builder

- [ ] Port/adapt data models from HarmonyDebugPanel:
  - `PatchInfo` — target method, patch method, owner, patch type, Harmony
    version
  - `ModInfo` — mod name, path, Harmony version, expected/actual patch counts
  - `AssemblyInfo` — name, version, is Harmony-related flag
  - `DiagnosticReport` — aggregated report with all sections
  - `PatchComparisonResult` — expected vs actual with missing/extra lists
- [ ] Create `IDiagnosticReportBuilder` interface
- [ ] Create `DiagnosticReportBuilder` (Category 1) — orchestrates collectors,
  builds `DiagnosticReport`
  - Accepts all collectors via interface injection
  - Performs expected vs actual patch comparison
  - Flags missing/failed patches with warnings
- [ ] Create `IReportFormatter` interface
  - `string FormatForOverlay(DiagnosticReport report)` — structured text for
    IMGUI
  - `string FormatForLogFile(DiagnosticReport report)` — detailed text for file
    export

### Phase 4: IMGUI overlay and lifecycle

- [ ] Create `DebugPanelBehaviour : MonoBehaviour` (Category 2)
  - `OnGUI()` — delegates to `IOverlayRenderer`
  - `Update()` — delegates to `IInputHandler`
  - Holds references to report builder and renderer
  - `DontDestroyOnLoad` to survive scene changes
- [ ] Create `IOverlayRenderer` interface and `DiagnosticOverlay` implementation
  - Port rendering logic from HarmonyDebugPanel's `DiagnosticOverlay.cs`
  - `GUI.Window()` — draggable window
  - `GUILayout.BeginScrollView()` — scrollable content
  - `GUI.contentColor` — colored text for Harmony versions (blue=1.x,
    green=2.x)
  - Section toggles based on config
  - "Generate Log" button
  - Environment mode indicator (standalone vs enhanced)
- [ ] Create `IInputHandler` interface and implementation
  - Hotkey detection (`Input.GetKeyDown`) for overlay toggle and refresh
  - Reads hotkey bindings from config
- [ ] Create startup patch to inject the `GameObject`
  - Patch an early game lifecycle method (e.g., `GameManager.Awake` or
    `TitleSceneController`)
  - In postfix: `new GameObject("DebugPanel").AddComponent<DebugPanelBehaviour>()`
  - Standard two-method pattern: extension method (testable setup logic) +
    `[EntryPoint]` postfix
- [ ] Create `ILogFileWriter` interface and implementation
  - Writes diagnostic report to timestamped log file in mod directory
  - Opens file in default text editor (or just writes, depending on platform)

### Phase 5: Testing

- [ ] Tests for `DiagnosticReportBuilder` — mock all collectors, verify report
  assembly and patch comparison logic
- [ ] Tests for `ExpectedPatchSource` — mock filesystem/reflection inputs,
  verify three-phase scan
- [ ] Tests for `HarmonyVersionClassifier` — various assembly reference
  scenarios
- [ ] Tests for `BaseModCollector` — mock filesystem, verify mod detection
- [ ] Tests for `ReportFormatter` — verify overlay and log file formatting
- [ ] Tests for `InputHandler` — verify hotkey config parsing
- [ ] Tests for config loading — missing file (defaults), valid file, malformed
  file
- [ ] Tests for collector factory/strategy — verify correct implementation
  selected based on environment
- [ ] Tests for patch comparison — expected vs actual matching, missing patch
  detection, extra patch detection
- [ ] Registration tests and exception logging tests for all Harmony patches

### Phase 6: Integration and migration

- [ ] Test manually with LMM/Basemod only (no BepInEx) — verify full standalone
  functionality
- [ ] Test manually with BepInEx installed — verify enhanced mode activates and
  shows Harmony 2 data
- [ ] Test with RetargetHarmony active — verify mixed Harmony 1/2 patch
  detection
- [ ] Document any behavioral differences from HarmonyDebugPanel
- [ ] Once validated, mark HarmonyDebugPanel as deprecated
  - Update its README/docs to point to DebugPanel
  - Do not delete immediately — keep for reference during transition

## Risks and Considerations

### Harmony 1.09 `HarmonySharedState` reflection

- `HarmonySharedState` is an internal implementation detail, not a public API.
  Its structure could theoretically differ across Harmony 1.x builds. However,
  since the game ships with a fixed Harmony 1.09 version, this is stable in
  practice.
- Need to verify the exact field names and types at runtime. If reflection fails,
  fall back gracefully (show "unable to read active patches" rather than
  crashing).

### `GameObject` injection timing

- The `MonoBehaviour` must be injected early enough to be available before the
  user might want diagnostics, but late enough that Unity is fully initialized.
- `DontDestroyOnLoad` is essential to survive scene transitions.
- If the patched method runs multiple times, guard against creating duplicate
  GameObjects.

### JsonUtility limitations for config

- `JsonUtility` cannot deserialize to a class with a parameterized constructor
  — must use default constructor + public fields.
- Missing JSON fields get default values (fine for our use case).
- No dictionary support — config must be flat fields only.
- Consider: if `JsonUtility` proves awkward for config, simple line-based
  key=value parsing (like the existing `.cfg` format) is an alternative that
  avoids Unity serialization constraints entirely.

### Reflection into BepInEx APIs

- All BepInEx/Harmony 2 reflection calls must be wrapped in try/catch. If the
  API surface changes between BepInEx versions, enhanced mode should degrade
  gracefully to standalone mode rather than crashing.
- Assembly loading order matters — BepInEx assemblies should be loaded by the
  time our mod initializes (Basemod loads after BepInEx chainloader), but this
  should be verified.

### Testing reflection-heavy code

- Category 2 wrappers will be thin but numerous. Each wrapper needs a
  corresponding interface. This is more boilerplate than typical mods but
  ensures the business logic (report building, comparison, formatting) is fully
  testable.
- Consider creating a shared `ReflectionHelper` utility in the mod (not Common)
  to reduce repetitive reflection patterns across collectors.

### Config file location

- The mod DLL lives in `BaseMods/{ModName}/`. The config file should live
  alongside it (same directory), matching how HarmonyDebugPanel stores config
  relative to the plugin.
- `IFileManager` already supports reading from the mod's directory.

### Feature parity gaps

- BepInEx `ConfigFile` supports runtime config reloading via file watcher.
  The JSON config approach would only read at startup (or on manual refresh).
  This is acceptable — config changes require a game restart or F10 refresh.
- HarmonyDebugPanel logs to `BepInEx/LogOutput.log` at startup. The standalone
  version would log to its own file in the mod directory. When BepInEx is
  available, consider also logging to BepInEx's logger via reflection.
