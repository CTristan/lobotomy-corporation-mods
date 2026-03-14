# Plan: Unified Debug Panel Mod

## Context

The existing `HarmonyDebugPanel` is a BepInEx 5 plugin that provides an in-game
IMGUI overlay and diagnostic logging for Harmony patch inspection. It works well
but **requires BepInEx to be installed**, which not all users have. Many users
run mods through LMM/Basemod with Harmony 1.09 only.

Additionally, as documented in `docs/harmony-interop-investigation.md`, BepInEx's
built-in `HarmonyInteropFix` **silently rewrites BaseMods DLLs on disk** — it
patches `Assembly.LoadFile()` with a prefix that uses Mono.Cecil to rename
`0Harmony` references to `0Harmony109` and overwrites the original file. This is
destructive and difficult to diagnose without tooling. Backups are created in
`{gameRoot}/BepInEx_Shim_Backup/` and a cache at
`BepInEx/cache/harmony_interop_cache.dat`, but users have no visibility into this.

This plan creates a new mod, `LobotomyCorporationMods.DebugPanel`, that:

1. **Always works** as a standard LMM/Basemod mod using Harmony 1.09
2. **Dynamically enhances** itself when BepInEx and Harmony 2 are detected at
   runtime, using their APIs as the single source of truth for patch data
3. **Detects DLL integrity issues** — flags when BaseMods DLLs have been
   rewritten on disk by HarmonyInteropFix or other processes
4. **Eventually replaces** `HarmonyDebugPanel` once proven reliable

### Existing infrastructure to reuse

- **HarmonyDebugPanel** (`HarmonyDebugPanel/`) — Reference implementation for
  collectors, overlay rendering, report building, and configuration
- **Common logging** (`LobotomyCorporationMods.Common`) — `ILogger`,
  `ILoggerTarget`, `FileLoggerTarget`, `IFileManager` interfaces
- **Mod pattern** — `Harmony_Patch.cs` singleton, two-method patch design,
  Category 1/2 class split
- **BaseModsAnalyzer** (`RetargetHarmony.Installer/Services/BaseModsAnalyzer.cs`)
  — Mono.Cecil DLL reading pattern with `ReaderParameters`
- **Interop investigation** (`docs/harmony-interop-investigation.md`) —
  HarmonyInteropFix behavior, backup paths, cache format

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
- [ ] DLL integrity detection: identify which BaseMods DLLs have been rewritten
  on disk, compare original vs current assembly references, flag missing backups
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
3. **Mono.Cecil**: Assembly named `Mono.Cecil` (needed for deep DLL inspection)

Detection results determine which collector implementations are used:

| Component | Standalone (Harmony 1.09) | Enhanced (BepInEx + Harmony 2) |
|-----------|--------------------------|-------------------------------|
| Active patches | `HarmonySharedState` reflection | `Harmony.GetAllPatchedMethods()` via reflection |
| Mod listing | Filesystem scan of `BaseMods/` | Filesystem scan + BepInEx plugin listing |
| Patcher status | Not available | RetargetHarmony detection |
| Patch data | Harmony 1.09 patches only | All patches (Harmony 1 + 2) |
| Assemblies | `AppDomain.GetAssemblies()` | Same |
| Expected patches | Reflection + source scanning | Same |
| DLL integrity | Raw byte scanning for Harmony refs | Mono.Cecil deep inspection |
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
- `showDllIntegrity` (bool, default `true`)

## Tasks

### Phase 1: Project scaffolding and configuration

- [x] Create `LobotomyCorporationMods.DebugPanel/` project (net35, SDK-style
  csproj)
  - References: `UnityEngine.dll`, `0Harmony109.dll`,
    `LobotomyCorporationMods.Common`
  - `<Private>false</Private>` on all external game DLL references
- [x] Create `Harmony_Patch.cs` following the standard singleton pattern
  extending `HarmonyPatchBase`
- [x] Define config JSON model class in `JsonModels/DebugPanelConfig.cs`
  - `[Serializable]`, public fields, PascalCase property accessors
  - All supported config fields with sensible defaults
- [x] Create `IConfigProvider` interface and `ConfigProvider` implementation
  - Category 2 wrapper for file I/O, reads/writes JSON config
  - Falls back to defaults if config file doesn't exist
- [x] Add test folder `LobotomyCorporationMods.Test/ModTests/DebugPanelTests/`

### Phase 2: Runtime detection, data models, and collector interfaces

- [x] Create data models (`Models/`):
  - `PatchType` enum, `HarmonyVersion` enum, `ModSource` enum
  - `PatchInfo`, `AssemblyInfo`, `DetectedModInfo` (renamed from `ModInfo` to
    avoid collision with game's global-namespace `ModInfo` in Assembly-CSharp),
    `ExpectedPatchInfo`, `RetargetHarmonyStatus`, `EnvironmentInfo`
- [x] Create DTOs (`Interfaces/`):
  - `PatchInspectionInfo`, `AssemblyInspectionInfo`,
    `BepInExPluginInspectionInfo`
- [x] Create interfaces (`Interfaces/`):
  - `IInfoCollector<T>`, `IEnvironmentDetector`, `IPatchInspectionSource`,
    `IAssemblyInspectionSource`, `IPluginInfoSource`, `IActivePatchCollector`,
    `IExpectedPatchSource`, `IHarmonyVersionClassifier`, `ICollectorFactory`
- [x] Create `EnvironmentDetector` (Category 2) — scans
  `AppDomain.CurrentDomain.GetAssemblies()` for known assembly names
- [x] Create `Harmony1PatchInspectionSource` (Category 2) — reflects into
  `Harmony.HarmonySharedState` to extract active patch data from Harmony 1.09
- [x] Create `Harmony2PatchInspectionSource` (Category 2) — reflects into
  `HarmonyLib.Harmony.GetAllPatchedMethods()` and `GetPatchInfo()`
- [x] Create `AppDomainAssemblyInspectionSource` (Category 2) — wraps
  `AppDomain.CurrentDomain.GetAssemblies()`
- [x] Create `ReflectionBepInExPluginInfoSource` (Category 2) — reflects into
  `BepInEx.Bootstrap.Chainloader.PluginInfos`
- [x] Create `ActivePatchCollector` (Category 1) — maps
  `PatchInspectionInfo` → `PatchInfo`
- [x] Create `AssemblyInfoCollector` (Category 1) — maps assemblies, detects
  Harmony-related
- [x] Create `HarmonyVersionClassifier` (Category 1) — classifies by assembly
  reference names
- [x] Create `BaseModCollector` (Category 1) — groups patches by assembly,
  filters framework assemblies
- [x] Create `BepInExPluginCollector` (Category 1) — maps plugins to
  `DetectedModInfo`
- [x] Create `RetargetHarmonyDetector` (Category 1) — checks for
  RetargetHarmony assembly
- [x] Create `ExpectedPatchSource` — three-phase scan ported from
  HarmonyDebugPanel (reflection, source file regex, runtime fallback)
- [x] Create `CollectorFactory` (Category 1) — selects implementations based
  on `IEnvironmentDetector` results
- [x] Add CA1031 suppression in `.editorconfig` for DebugPanel project
  (reflection code intentionally catches general exceptions)
- [x] Verify: solution builds, all existing tests pass

Note: `DetectedModInfo` was renamed from `ModInfo` because Assembly-CSharp.dll
contains a global-namespace `ModInfo` class. C# name resolution finds global
types before checking `using` directives, causing CS1729 errors.

### Phase 3: Report builder and remaining models

- [x] Data models ported in Phase 2 (`PatchInfo`, `DetectedModInfo`,
  `AssemblyInfo`, `ExpectedPatchInfo`, `RetargetHarmonyStatus`,
  `EnvironmentInfo`)
- [x] Create `DiagnosticReport` model — aggregated report with all sections
- [x] Create `PatchComparisonResult` model — expected vs actual with
  missing/extra lists
- [x] Create `MissingPatchInfo` model — represents expected patch not found at
  runtime
- [x] Create `IDiagnosticReportBuilder` interface
- [x] Create `DiagnosticReportBuilder` (Category 1) — orchestrates collectors,
  builds `DiagnosticReport`
  - Accepts `ICollectorFactory` + `IEnvironmentDetector` via interface injection
  - Performs expected vs actual patch comparison
  - Flags missing/failed patches with warnings
  - Correlates patches with originating mods
- [x] Create `IReportFormatter` interface
  - `IList<string> FormatForOverlay(DiagnosticReport report)` — compact text
    for IMGUI
  - `IList<string> FormatForLogFile(DiagnosticReport report)` — detailed text
    for file export
- [x] Create `ReportFormatter` (Category 1) — implements `IReportFormatter`
  with overlay/log formatting, environment mode labels, patch status display
- [x] Tests for all Phase 3 models and implementations
- [x] Verify: solution builds, all existing tests pass

### Phase 4: IMGUI overlay and lifecycle

- [x] Create `DebugPanelBehaviour : MonoBehaviour` (Category 2)
  - `OnGUI()` — delegates to `IOverlayRenderer`
  - `Update()` — delegates to `IInputHandler`
  - Holds references to report builder and renderer
  - `DontDestroyOnLoad` to survive scene changes
- [x] Create `IOverlayRenderer` interface and `DiagnosticOverlay` implementation
  - Port rendering logic from HarmonyDebugPanel's `DiagnosticOverlay.cs`
  - `GUI.Window()` — draggable window
  - `GUILayout.BeginScrollView()` — scrollable content
  - `GUI.contentColor` — colored text for Harmony versions (blue=1.x,
    green=2.x)
  - Section toggles based on config
  - "Generate Log" button
  - Environment mode indicator (standalone vs enhanced)
- [x] Create `IInputHandler` interface and implementation
  - Hotkey detection (`Input.GetKeyDown`) for overlay toggle and refresh
  - Reads hotkey bindings from config
- [x] Create startup patch to inject the `GameObject`
  - Patches `GlobalGameManager.Awake` (private method, earliest lifecycle)
  - In postfix: `new GameObject("DebugPanelBehaviour").AddComponent<DebugPanelBehaviour>()`
  - Standard two-method pattern: extension method (testable setup logic) +
    `[EntryPoint]` postfix
  - Duplicate guard via `FindObjectOfType<DebugPanelBehaviour>()`
- [x] Create `ILogFileWriter` interface and `LogFileWriter` implementation
  (Category 1)
  - Writes diagnostic report to timestamped log file in mod directory
  - Uses `IFileManager` + `IReportFormatter` for full testability
- [x] Create `KeyCodeParser` (Category 1) — parses config key strings
  ("F9", "F10", etc.) to `KeyCode` enum values
- [x] Add `UnityEngine.IMGUIModule` and `UnityEngine.InputModule` references
  to csproj
- [x] Add `PrivateMethods.GlobalGameManager.Awake` constant to Common
- [x] Tests for `KeyCodeParser` — function keys, common keys, case
  insensitivity, defaults for null/empty/unrecognized
- [x] Tests for `LogFileWriter` — null guards, formatting, file writing,
  filename generation
- [x] Registration test and exception logging test for
  `GlobalGameManagerPatchAwake`
- [x] Business logic tests for `PatchAfterAwake` extension method
- [x] Verify: solution builds, all 396 tests pass

### Phase 5: Testing

- [ ] Tests for `DiagnosticReportBuilder` — mock all collectors, verify report
  assembly and patch comparison logic
- [ ] Tests for `ExpectedPatchSource` — mock filesystem/reflection inputs,
  verify three-phase scan
- [ ] Tests for `HarmonyVersionClassifier` — various assembly reference
  scenarios
- [ ] Tests for `BaseModCollector` — mock inspection source, verify mod detection
- [ ] Tests for `ReportFormatter` — verify overlay and log file formatting
- [ ] Tests for `InputHandler` — verify hotkey config parsing
- [ ] Tests for config loading — missing file (defaults), valid file, malformed
  file
- [ ] Tests for collector factory/strategy — verify correct implementation
  selected based on environment
- [ ] Tests for patch comparison — expected vs actual matching, missing patch
  detection, extra patch detection
- [ ] Registration tests and exception logging tests for all Harmony patches

### Phase 6: DLL integrity detection — source interfaces

- [ ] Create `IDllFileInspector` interface
  - `IList<string> GetAssemblyReferences(string dllPath)` — returns assembly
    reference names found in a DLL file on disk
  - `bool IsDeepInspectionAvailable { get; }` — true when using Mono.Cecil
- [ ] Create `CecilDllFileInspector` (Category 2) — uses
  `AssemblyDefinition.ReadAssembly()` for precise reference extraction
  - Pattern from `BaseModsAnalyzer.cs`: `ReadAssembly(path, new ReaderParameters { ReadWrite = false })`
  - Handle `BadImageFormatException`, `IOException`, `InvalidOperationException`
  - Only instantiated when `IEnvironmentDetector.IsMonoCecilAvailable` is true
- [ ] Create `BasicDllFileInspector` (Category 2) — standalone fallback
  - Reads raw DLL bytes via `File.ReadAllBytes()`
  - Scans for known UTF-8 byte sequences: `0Harmony109`, `0Harmony12`,
    `12Harmony`, `0Harmony` (excluding matches that are substrings of longer
    names)
  - Less precise than Cecil (could match string literals), but sufficient for
    detecting shimming
  - `IsDeepInspectionAvailable` returns `false`
- [ ] Create `IShimArtifactSource` interface
  - `bool BackupDirectoryExists { get; }`
  - `string BackupDirectoryPath { get; }`
  - `IList<string> GetBackupFileNames()` — list files in backup dir
  - `byte[] ReadBackupFileBytes(string fileName)` — read a backup DLL
  - `bool InteropCacheExists { get; }`
  - `string InteropCachePath { get; }`
  - `int GetInteropCacheEntryCount()` — parse binary cache, return count or -1
- [ ] Create `FileSystemShimArtifactSource` (Category 2) — looks for
  `{gameRoot}/BepInEx_Shim_Backup/` and
  `{gameRoot}/BepInEx/cache/harmony_interop_cache.dat`
  - Game root derived from `Application.dataPath` (up one level from `_Data/`)
  - Cache format: `BinaryReader` with string-long pairs; wrap in try/catch
- [ ] Create `ILoadedAssemblyReferenceSource` interface
  - `IList<LoadedAssemblyInfo> GetBaseModAssemblies()` — returns name, location,
    and reference names for assemblies loaded from the BaseMods directory
- [ ] Create `AppDomainLoadedAssemblySource` (Category 2) — filters
  `AppDomain.CurrentDomain.GetAssemblies()` to those loaded from `BaseMods/`

### Phase 7: DLL integrity detection — data models and collector

- [ ] Create `FindingSeverity` enum — `Info`, `Warning`, `Error`
- [ ] Create `LoadedAssemblyInfo` data class — `Name`, `Location`,
  `References` (list of reference names)
- [ ] Create `DllIntegrityFinding` model:
  - `DllPath` (string) — full path to DLL on disk
  - `DllName` (string) — filename only
  - `FindingSeverity` (enum)
  - `OnDiskHarmonyReferences` (IList<string>) — references found on disk
  - `OriginalHarmonyReferences` (IList<string>) — references from backup copy
  - `HasBackup` (bool) — whether backup exists in `BepInEx_Shim_Backup/`
  - `BackupPath` (string)
  - `WasRewritten` (bool) — true if on-disk differs from backup/original
  - `Summary` (string) — human-readable description
- [ ] Create `DllIntegrityReport` model:
  - `Findings` (IList<DllIntegrityFinding>)
  - `ShimBackupDirectoryExists` (bool)
  - `ShimBackupDirectoryPath` (string)
  - `InteropCacheExists` (bool)
  - `InteropCachePath` (string)
  - `InteropCacheEntryCount` (int) — -1 if unreadable
  - `MonoCecilAvailable` (bool)
  - `TotalRewrittenCount` (int)
  - `Warnings` (IList<string>)
  - `Summary` (string) — overall status
- [ ] Create `DllIntegrityCollector` implementing
  `IInfoCollector<DllIntegrityReport>` (Category 1)
  - Constructor takes `IDllFileInspector`, `IShimArtifactSource`,
    `ILoadedAssemblyReferenceSource`
  - Algorithm:
    1. Check shim backup dir and interop cache existence
    2. Get loaded BaseMod assemblies from `ILoadedAssemblyReferenceSource`
    3. For each: read on-disk references via `IDllFileInspector`, check for
       backup, read backup references if available, compare
    4. Classify severity per finding:

       | Condition | Severity | Summary |
       |-----------|----------|---------|
       | References unchanged, no backup | Info | "Not modified" |
       | Rewritten to `0Harmony109`, backup exists | Warning | "Rewritten by BepInEx shim (backup available)" |
       | Rewritten to `0Harmony109`, no backup | Error | "Rewritten by BepInEx shim (no backup!)" |
       | Unexpected reference name | Error | "Unexpected Harmony reference: {name}" |
       | DLL unreadable | Warning | "Unable to read DLL: {error}" |

    5. Build summary message
- [ ] Add `DllIntegrityReport DllIntegrity` property to `DiagnosticReport`
- [ ] Update `DiagnosticReportBuilder` to include `DllIntegrityCollector`
- [ ] Update collector factory to create appropriate `IDllFileInspector` based
  on `IEnvironmentDetector.IsMonoCecilAvailable`

### Phase 8: DLL integrity detection — overlay and formatting

- [ ] Add `DrawDllIntegritySection` to `DiagnosticOverlay`
  - Rendered after RetargetHarmony status, before Active Patches section
  - Toggleable via `showDllIntegrity` config field
  - Color coding:
    - Green (`0.66f, 1f, 0.66f`) — Info (clean)
    - Orange (`1f, 0.6f, 0.4f`) — Warning (rewritten with backup)
    - Red (`1f, 0.4f, 0.4f`) — Error (rewritten without backup, unexpected)
  - Layout:
    ```
    DLL Integrity (3 checked, 1 rewritten)
      [green]  ModA.dll — Not modified
      [orange] ModB.dll — Rewritten by BepInEx shim (backup available)
               On-disk: 0Harmony109 | Original: 0Harmony
      [red]    ModC.dll — Rewritten (no backup!)

      Shim Backup: BepInEx_Shim_Backup/ (2 files)
      Interop Cache: harmony_interop_cache.dat (5 entries)
      Inspection Mode: Deep (Mono.Cecil) | Basic (byte scan)
    ```
- [ ] Add DLL integrity section to `IReportFormatter.FormatForLogFile()`

### Phase 9: DLL integrity detection — testing

- [ ] `DllIntegrityCollector` tests with mocked sources — all severity
  combinations:
  - Clean DLL (no shimming, no backup) → Info
  - Shimmed DLL with backup available → Warning
  - Shimmed DLL without backup → Error
  - Unexpected reference name → Error
  - Unreadable DLL → Warning with error message
  - Empty BaseMods directory → empty report
- [ ] `BasicDllFileInspector` tests — byte arrays with known patterns:
  - Contains `0Harmony` only → detected
  - Contains `0Harmony109` → detected (distinguishes from `0Harmony`)
  - Contains both → both detected
  - No Harmony references → empty list
  - Partial match / string in non-reference context → handled appropriately
- [ ] Severity classification boundary tests
- [ ] Graceful degradation when Mono.Cecil unavailable — verify factory selects
  `BasicDllFileInspector`, findings note "approximate" mode
- [ ] Shim artifact detection tests — backup dir exists/missing, cache
  readable/corrupt/missing
- [ ] `DllIntegrityReport` summary generation tests

### Phase 10: Integration and migration

- [ ] Test manually with LMM/Basemod only (no BepInEx) — verify full standalone
  functionality
- [ ] Test manually with BepInEx installed — verify enhanced mode activates and
  shows Harmony 2 data
- [ ] Test with RetargetHarmony active — verify mixed Harmony 1/2 patch
  detection
- [ ] Test DLL integrity detection:
  - With unmodified BaseMods DLLs (no BepInEx) → all green/Info
  - With BepInEx after first launch (DLLs shimmed) → orange/Warning with
    backup comparison
  - After deleting `BepInEx_Shim_Backup/` (shimmed, no backup) → red/Error
  - Verify overlay colors and log file output match expected severity
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

### Mono.Cecil availability for DLL inspection

- Mono.Cecil is only present at runtime when BepInEx is installed. The
  `BasicDllFileInspector` fallback uses string matching on raw bytes, which
  could produce false positives if `0Harmony` appears in string literals or
  other metadata. Mitigations: mark findings as "approximate" in basic mode,
  and match reference-length byte patterns with null terminators where possible.

### Backup directory collisions

- `BepInEx_Shim_Backup/` stores files by filename only. If two mods have
  identically named DLLs in different subdirectories, backups could collide.
  Treat missing/mismatched backups as "no backup" rather than crashing.

### Interop cache binary format

- The cache uses `BinaryReader`/`BinaryWriter` with string-long pairs. Wrap
  reads in try/catch, return -1 entry count on failure. This is informational,
  not critical for integrity detection.

### DLL integrity detection timing

- By the time our mod loads (via `Add_On.init()` → `Assembly.LoadFile()`),
  BepInEx shimming has already completed. All detection is after-the-fact.
  We cannot intercept shimming in-flight from a Harmony 1.09 mod, but we can
  fully detect and report what happened.
