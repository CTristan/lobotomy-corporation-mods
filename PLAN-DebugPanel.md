# Plan: DebugPanel Troubleshooting Features

## Context

The Lobotomy Corporation modding community has a [detailed troubleshooting wiki](https://lobotomycorporationmodded.wiki.gg/wiki/Troubleshooting) documenting dozens of common issues users encounter. Currently, diagnosing these problems requires manually inspecting file structures, reading multiple log files, and performing trial-and-error mod removal. The DebugPanel mod already provides strong Harmony patch diagnostics and DLL integrity checking, but doesn't address the most common user-facing issues: misplaced files, missing localization, known problematic mods, error log detection, and dependency problems.

This plan adds comprehensive troubleshooting features that proactively detect and surface these issues, targeting mod developers, end users, and other mod authors equally.

## Goals

- Detect the most common issues from the troubleshooting wiki automatically at runtime
- Surface issues with clear descriptions and actionable fix suggestions
- Organize the growing diagnostic information into a navigable tabbed UI with expandable sections
- Maintain the existing architecture patterns (interface-first, Category 1/2 split, `IInfoCollector<T>`)
- Ship a community-maintainable known issues database as an external JSON file
- 100% test coverage on all new business logic

## Approach

Add four new collector types (filesystem validation, error logs, known issues, dependencies), an issues aggregator, and overhaul the overlay UI from a single scrollable window to a tabbed layout with expandable sections. New data models go in `Common/Models/Diagnostics/`. Implementation follows the existing `IInfoCollector<T>` pattern. A new `IFileSystemScanner` interface abstracts all filesystem access for testability.

## Tasks

### Phase 1: Foundation — Data Models + Filesystem Abstraction

- [ ] Create `DiagnosticIssue` model class in `Common/Models/Diagnostics/`
  - Properties: `FindingSeverity Severity`, `string Category`, `string Description`, `string SourceTab`, `string FixSuggestion`
  - Follows existing model pattern (sealed, ThrowHelper null checks, private setters)
- [ ] Create `FilesystemValidationReport` model in `Common/Models/Diagnostics/`
  - Properties: `IList<DiagnosticIssue> Issues`, `string Summary`
- [ ] Create `ErrorLogEntry` model in `Common/Models/Diagnostics/`
  - Properties: `string FileName`, `string Content`, `string FilePath`
- [ ] Create `ErrorLogReport` model in `Common/Models/Diagnostics/`
  - Properties: `IList<ErrorLogEntry> Entries`
- [ ] Create `KnownIssueMatch` model in `Common/Models/Diagnostics/`
  - Properties: `string ModName`, `FindingSeverity Severity`, `string Description`, `string FixSuggestion`, `string WikiLink`, `string MatchedBy`
- [ ] Create `KnownIssuesReport` model in `Common/Models/Diagnostics/`
  - Properties: `IList<KnownIssueMatch> Matches`, `string DatabaseVersion`
- [ ] Create `DependencyReport` model in `Common/Models/Diagnostics/`
  - Properties: `IList<DiagnosticIssue> Issues`, `string BaseModVersion`, `bool BaseModListExists`
- [ ] Create `IFileSystemScanner` interface in `DebugPanel/Interfaces/`
  - Methods: `DirectoryExists`, `FileExists`, `GetFiles`, `GetDirectories`, `ReadAllText`, `GetFileSize`
  - Path accessors: `GetBaseModsPath()`, `GetSaveDataPath()`, `GetGameRootPath()`, `GetExternalDataPath()`
- [ ] Create `GameFileSystemScanner` in `DebugPanel/Implementations/` (Category 2: `[AdapterClass]` + `[ExcludeFromCodeCoverage]`)
  - Derives paths from `Application.dataPath`, wraps `System.IO`
- [ ] Expand `DiagnosticReport` constructor with new optional properties (default to empty collections/null-safe values)
  - Add: `FilesystemValidationReport`, `ErrorLogReport`, `KnownIssuesReport`, `DependencyReport`, `IList<DiagnosticIssue> AggregatedIssues`
- [ ] Update all existing `DiagnosticReport` call sites (builder + tests)
- [ ] Write model tests for all new classes

**Critical files:**
- `Common/Models/Diagnostics/DiagnosticReport.cs` — expand constructor
- `DebugPanel/Implementations/DiagnosticReportBuilder.cs` — update call site
- `Test/ModTests/DebugPanelTests/DiagnosticReportBuilderTests.cs` — update test call sites

### Phase 2: Filesystem Validation + Error Log Detection

- [ ] Create `IInfoCollector<FilesystemValidationReport>` implementation: `FilesystemValidationCollector`
  - Constructor: `(IFileSystemScanner scanner)`
  - Checks:
    - `Assembly-CSharp.dll` present in BaseMods folder (Error: assembly replacement detected)
    - LMM executables (`LobotomyModManager.exe`) or BaseMod DLLs inside BaseMods (Error: known to cause Infohazard)
    - Double-folder nesting: mod subdirectory contains only one child that is also a directory containing a DLL (Warning)
    - Save data path length > 260 characters (Warning: Windows MAX_PATH)
    - Extra non-standard files in `Project_Moon/` save folder (Warning: degrades backup performance)
    - `BaseModList_v2.xml` missing or empty (Warning: mod list may not load)
- [ ] Create `IInfoCollector<ErrorLogReport>` implementation: `ErrorLogCollector`
  - Constructor: `(IFileSystemScanner scanner)`
  - Scans BaseMods for: `Herror.txt`, `LMMerror.txt`, `GlError.txt`, `DllError.txt`, `LTDerror.txt`, `DPerror.txt`, `Glerror.txt`
  - Reads file content and exposes for display/log
- [ ] Add factory methods to `ICollectorFactory` and `CollectorFactory`:
  - `CreateFilesystemValidationCollector()`
  - `CreateErrorLogCollector()`
- [ ] Integrate into `DiagnosticReportBuilder.BuildReport()` (steps 9-10, independent of existing collectors)
- [ ] Add new sections to `ReportFormatter.FormatForLogFile()`
- [ ] Write tests: `FilesystemValidationCollectorTests.cs`, `ErrorLogCollectorTests.cs`

**Critical files:**
- `DebugPanel/Interfaces/ICollectorFactory.cs` — add methods
- `DebugPanel/Implementations/CollectorFactory.cs` — implement new methods, accept `IFileSystemScanner`
- `DebugPanel/Implementations/DiagnosticReportBuilder.cs` — call new collectors
- `DebugPanel/Implementations/ReportFormatter.cs` — format new sections

### Phase 3: Known Issues Database + Dependency Checking

- [ ] Create JsonUtility-compatible models in `DebugPanel/JsonModels/`:
  - `KnownIssuesData`: `string version`, `string lastUpdated`, `KnownIssueItem[] issues`
  - `KnownIssueItem`: `string modName`, `string dllName`, `string assemblyName`, `int severity`, `string description`, `string fixSuggestion`, `string wikiLink`, `string[] conflictsWith`
  - Note: no Dictionary (JsonUtility constraint), use arrays
- [ ] Create `IKnownIssuesDatabase` interface in `DebugPanel/Interfaces/`
  - `IList<KnownIssueItem> GetKnownIssues()`, `string DatabaseVersion`
- [ ] Create `JsonKnownIssuesDatabase` implementation
  - Loads `known-issues.json` via `IFileSystemScanner.ReadAllText()` + `JsonUtility.FromJson<KnownIssuesData>()`
- [ ] Create `known-issues.json` data file with initial entries from the wiki:
  - **Error-level**: Memory Leak Fix (assembly replacement in BaseMods), Assembly-CSharp.dll replacements
  - **Warning-level**: ALotOptimization (crashes, invisible agents/ordeals), Weapon Texture Fix (breaks modded EGO), Agent Delete (NullRef on agent select), Bamboo Hatted Kim (numerous bugs)
  - **Info-level**: Save Data Profiles (new/unstable), Colored Fixer (known stall bug, bugfix available)
  - **Conflicts**: Suppress Leveling + Weapon Remakes, NewGiftBox + The King in Binds/QingYuXie, Gift Alert Icon + The King in Binds/QingYuXie, both optimization mods active simultaneously
  - **Dependencies**: mods referencing `12Harmony` (include fix suggestion)
- [ ] Create `KnownIssuesChecker` implementing `IInfoCollector<KnownIssuesReport>`
  - Constructor: `(IKnownIssuesDatabase database, IList<DetectedModInfo> detectedMods, IList<AssemblyInfo> loadedAssemblies, IFileSystemScanner scanner)`
  - Matches by DLL filename in BaseMods and by loaded assembly name
  - Checks conflict pairs (both sides present)
- [ ] Create `DependencyChecker` implementing `IInfoCollector<DependencyReport>`
  - Constructor: `(IFileSystemScanner scanner, IList<DetectedModInfo> detectedMods, IList<AssemblyInfo> loadedAssemblies)`
  - Checks: missing `12Harmony.dll` when referenced, missing `CustomWorkBaseMod`, BaseMod version detection, `BaseModList_v2.xml` health
- [ ] Add factory methods and integrate into builder (these depend on mods/assemblies, so run after steps 1-4)
- [ ] Add to `ReportFormatter`
- [ ] Include `known-issues.json` in csproj as content (`<Content>`)
- [ ] Write tests: `JsonKnownIssuesDatabaseTests.cs`, `KnownIssuesCheckerTests.cs`, `DependencyCheckerTests.cs`

### Phase 4: Issues Aggregator + UI Overhaul

- [ ] Create `IIssuesAggregator` interface + `IssuesAggregator` implementation
  - `IList<DiagnosticIssue> AggregateIssues(DiagnosticReport report)`
  - Walks all report sections, extracts/converts to `DiagnosticIssue`
  - Existing warnings → Info severity issues
  - Missing patches → Warning severity issues
  - DLL integrity errors → mapped by `FindingSeverity`
  - New report sections contribute their own `DiagnosticIssue` lists directly
  - Output sorted by severity (Error → Warning → Info)
- [ ] Integrate into `DiagnosticReportBuilder` (runs last, populates `AggregatedIssues`)
- [ ] Rewrite `DiagnosticOverlay` to tabbed layout:
  - Tab bar using `GUILayout.Toolbar` or manual button row with issue count badges
  - **Tab 0 — Issues**: all `AggregatedIssues`, color-coded by severity, showing category + description + fix suggestion
  - **Tab 1 — Harmony**: existing patch sections (active patches, missing patches, expected patches)
  - **Tab 2 — Files**: filesystem validation, DLL integrity, error logs
  - **Tab 3 — Mods**: detected mods (BepInEx + LMM), known issues matches, dependency report
  - **Tab 4 — Environment**: assemblies, RetargetHarmony status, environment info, warnings
- [ ] Add expandable section support:
  - Each section header is a toggle button
  - Track expand state in `bool[]` or `Dictionary<string, bool>` (internal to overlay, no serialization needed)
  - Sections with issues auto-expand on report refresh; clean sections collapse
- [ ] Add new config fields to `DebugPanelConfig`:
  - `showFilesystemValidation`, `showErrorLogs`, `showKnownIssues`, `showDependencies` (all default `true`)
  - Existing booleans map to section visibility within their tabs
- [ ] Write tests: `IssuesAggregatorTests.cs`

### Phase 5 (Stretch): Localization & XML Validation

- [ ] Create `LocalizationReport` model
- [ ] Create `LocalizationValidator` implementing `IInfoCollector<LocalizationReport>`
  - Scan mod directories for `kr` folder without corresponding `en` folder
  - Detect missing `Skills_en.xml`, `AgentNewLyrics_en.xml`, `SefiraDesc_en.xml` equivalents
  - Basic XML well-formedness check (illegal `&` characters, unclosed tags)
  - Detect missing portrait files referenced in `CreatureInfo` XMLs
- [ ] Integrate into Files tab
- [ ] Write tests

## Architecture Notes

### How new collectors fit the existing pattern

All new collectors follow the same `IInfoCollector<T>` interface that existing collectors use. The `CollectorFactory` creates them with their dependencies injected. The `DiagnosticReportBuilder` orchestrates collection in order — new filesystem/error log collectors are independent (can run anytime), while known issues and dependency checkers depend on the mod/assembly collectors running first.

### Filesystem abstraction

A new `IFileSystemScanner` provides testable filesystem access. This is separate from `IShimArtifactSource` (which is specialized for BepInEx shim paths) because the new checks scan BaseMods, ExternalData, save folders, and mod directories.

### Known issues JSON format

Uses JsonUtility-compatible arrays (no Dictionary). The `known-issues.json` file ships alongside the mod DLL and can be updated independently. Structure:

```json
{
  "version": "1.0",
  "lastUpdated": "2026-03-16",
  "issues": [
    {
      "modName": "Memory Leak Fix",
      "dllName": "Assembly-CSharp.dll",
      "assemblyName": "",
      "severity": 2,
      "description": "Assembly replacement, not a mod. Causes conflicts preventing mod loading.",
      "fixSuggestion": "Remove from BaseMods. Use 'Reticle Memory Leak Fix' instead.",
      "wikiLink": "https://lobotomycorporationmodded.wiki.gg/wiki/Troubleshooting",
      "conflictsWith": []
    }
  ]
}
```

### DiagnosticReport expansion

New properties added to `DiagnosticReport` constructor with null-safe defaults. Existing code paths pass empty/default values until they're wired up in their respective phases. This avoids breaking changes across phases.

### UI state management

Tab index and section expand states are internal to `DiagnosticOverlay` (Category 2, `[ExcludeFromCodeCoverage]`). They reset on each game session but persist across report refreshes. Section auto-expand logic runs on each `Draw()` call based on the current report data.

## Verification

After each phase:

1. `dotnet build LobotomyCorporationMods.sln` — verify compilation
2. `dotnet test /p:CollectCoverage=true /p:CoverletOutput="./coverage.opencover.xml" /p:CoverletOutputFormat=opencover LobotomyCorporationMods.sln` — verify tests pass with coverage
3. `dotnet ci --check` — verify formatting and test compliance

End-to-end validation (after Phase 4):
- Deploy mod to game installation
- Open debug panel (F9) and verify tabbed layout renders
- Check Issues tab shows any detected problems
- Navigate each tab, verify sections expand/collapse
- Generate log file and verify all new sections appear
- Test with known problematic setups (e.g., place a dummy `Assembly-CSharp.dll` in BaseMods)

## Risks & Considerations

- **DiagnosticReport constructor breaking change**: Phase 1 modifies the constructor signature, requiring updates to all existing call sites (builder, tests, formatter). Do this first to avoid cascading conflicts.
- **JsonUtility limitations**: No Dictionary support means the known issues DB uses arrays. Lookups by DLL/assembly name require linear scans, but the list is small (<50 entries) so performance is fine.
- **.NET 3.5 constraints**: No LINQ `Where`/`Select`, no string interpolation, no `nameof`. Use explicit loops and string concatenation.
- **IMGUI tab rendering**: `GUILayout.Toolbar` returns an int index. The overlay is `[ExcludeFromCodeCoverage]` so the tab/section logic can't be unit tested — keep it as thin as possible, delegating data formatting to `ReportFormatter`.
- **Filesystem access timing**: Collectors run when the report is built (on game start + periodic refresh). File system state can change between refreshes. Error handling wraps each check independently so one failure doesn't block others.
- **Known issues maintenance**: The JSON file will need community updates as new problematic mods are discovered. Consider adding a version/date field so users know how current it is.
