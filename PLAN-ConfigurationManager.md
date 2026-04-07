# Plan: Shared Configuration System + BepInEx-Free ConfigurationManager Fork

## Context

Currently, only the DebugPanel mod uses reflection/lazy loading to detect BepInEx and enable optional functionality. The goal is to extract shared configuration abstractions into `LobotomyCorporationMods.Common` so that **all mods** can register configurable settings (with sensible defaults) and optionally expose them via a GUI when the user has ConfigurationManager installed.

The original BepInEx ConfigurationManager only works with BepInEx plugins (`BaseUnityPlugin`). Since our mods are Basemod/Harmony mods (not BepInEx plugins), we're forking ConfigurationManager to strip the BepInEx dependency and use a registration-based system from Common instead. The fork lives in a separate LGPL v3 repo as a git submodule, keeping our mods MIT-licensed.

**Example use case:** BadLuckProtectionForGifts could expose settings like "bonus probability per work" (default 1%) and "reset bonus when gift received" (default true) — configurable at runtime when ConfigurationManager is installed, otherwise using defaults.

## Goals

- Mods register settings via Common's API during initialization; settings work with defaults even without ConfigurationManager
- A forked ConfigurationManager renders an IMGUI settings GUI, discovers settings from Common's registry, and persists user changes to INI files
- The fork is a standalone Basemod DLL (MonoBehaviour, not BaseUnityPlugin) — no BepInEx required
- License separation: Common + mods remain MIT; fork is LGPL v3 in its own repo/submodule
- Build, CI, and deployment infrastructure updated to handle the submodule

## Approach

Two-layer architecture:

1. **Common** (MIT) — defines config interfaces + a static registry. No BepInEx dependency. Mods only interact with this layer.
2. **Fork** (LGPL v3, submodule) — the IMGUI GUI that reads from Common's registry and persists values. Optional — mods work without it.

The fork replaces BepInEx types with Common's abstractions and uses standard Harmony/Basemod loading.

## Tasks

### Phase 1: Common Config API

These files go in `LobotomyCorporationMods.Common/`.

- [ ] Create `Interfaces/IConfigurationEntry.cs`
  - Properties: `ModId`, `Section`, `Key`, `DisplayName`, `Description`, `SettingType`, `DefaultValue`, `AcceptableValues` (object[]), `AcceptableValueRange` (KeyValuePair<object,object>?), `IsAdvanced`, `Order`
  - Methods: `object Get()`, `void Set(object value)`

- [ ] Create `Interfaces/IConfigurationProvider.cs`
  - Seam for ConfigurationManager to write values and persist
  - Methods: `void LoadPersistedValues(IEnumerable<IConfigurationEntry> entries)`, `void Save()`

- [ ] Create `Implementations/ConfigurationModInfo.cs`
  - Simple data class replacing `BepInPlugin`: `ModId`, `ModName`, `ModVersion`
  - Category 1 (constructor takes only primitives)

- [ ] Create `Implementations/ConfigurationEntry.cs`
  - Concrete `IConfigurationEntry` implementation (Category 1)
  - Stores current value; `Get()` returns current or default; `Set()` validates against acceptable values/range

- [ ] Create `Implementations/ConfigurationRegistry.cs`
  - Static class following `DefaultLocalizedValues` pattern
  - Static dictionary keyed by `modId:section:key`
  - `Register(...)` — returns `IConfigurationEntry`
  - `GetAllEntries()` — for ConfigurationManager to enumerate
  - `GetEntriesForMod(string modId)` — entries grouped by mod
  - `SetProvider(IConfigurationProvider)` — called by ConfigurationManager on init
  - Thread-safe via `lock` (same pattern as `HarmonyPatchBase.ValidateThatStaticInstanceIsNotDuplicated`)

- [ ] Create `Implementations/ConfigurationEntryBuilder.cs`
  - Fluent builder API for ergonomic registration:
    ```csharp
    ConfigurationEntryBuilder.ForMod("ModId")
        .InSection("General")
        .WithKey("BonusPerWork")
        .OfType(typeof(float))
        .WithDefault(0.01f)
        .WithDescription("Probability bonus per work")
        .WithRange(0.001f, 0.1f)
        .Register();
    ```

- [ ] Write tests in `LobotomyCorporationMods.Test/`
  - `ConfigurationEntry`: value storage, defaults, range validation, acceptable values
  - `ConfigurationRegistry`: register, get, enumerate, thread safety, provider lifecycle
  - `ConfigurationEntryBuilder`: fluent API, required field validation

- [ ] Bump Common's `AssemblyVersion` in csproj

### Phase 2: Fork Repository Setup

Work in the fork repo (`CTristan/LobotomyCorporationMods.ConfigurationManager`).

- [ ] Create fork's `Directory.Build.props`
  - Standalone — must block MSBuild from walking up to the main repo's props
  - Replicate relevant settings: `TreatWarningsAsErrors`, `AnalysisLevel`, `Deterministic`, etc.

- [ ] Create fork's `.csproj` targeting net35
  - `ProjectReference` to Common (relative path: `../../LobotomyCorporationMods.Common/...`)
  - Game DLL references via `external/LobotomyCorp_Data/Managed/` (same as other mods, `<Private>false</Private>`)

- [ ] Create `Harmony_Patch.cs` — Basemod entry point
  - Sealed singleton extending `HarmonyPatchBase`
  - In init: creates `GameObject("ConfigurationManager").AddComponent<ConfigurationManagerBehaviour>()`

- [ ] Create `ConfigurationManagerBehaviour.cs` — MonoBehaviour (Category 2)
  - `[AdapterClass]` + `[ExcludeFromCodeCoverage]`
  - Delegates `Start()`, `Update()`, `OnGUI()` to `ConfigurationManagerCore`

- [ ] Rewrite `ConfigurationManager.cs` → `ConfigurationManagerCore.cs`
  - Strip `BaseUnityPlugin` inheritance — plain class
  - Replace `Config.Bind()` for own settings with `ConfigurationRegistry.Register()`
  - Replace `BepInPlugin` metadata with `ConfigurationModInfo`
  - Replace `ManualLogSource` with Common's `ILogger`
  - Strip all `#if IL2CPP` blocks
  - Keep all IMGUI window/rendering logic (it's BepInEx-free)
  - Implements `IConfigurationProvider`

- [ ] Create `RegistrySettingEntry.cs` — replaces `ConfigSettingEntry`
  - Wraps `IConfigurationEntry` from Common
  - Extends `SettingEntryBase` — maps properties through to the entry

- [ ] Rewrite `SettingSearcher.cs`
  - Replace `FindPlugins()` (Chainloader scan) with `ConfigurationRegistry.GetAllEntries()` grouped by mod
  - Replace `GetPluginConfig()` with registry enumeration
  - Remove BepInEx core config discovery

- [ ] Update `SettingEntryBase.cs`
  - Replace `BepInPlugin PluginInfo` with `ConfigurationModInfo PluginInfo`
  - Remove `BaseUnityPlugin PluginInstance`
  - Keep `SetFromAttributes()` reflection-based attribute copying (works without BepInEx)

- [ ] Update `SettingFieldDrawer.cs`
  - Replace `BepInEx.Configuration.KeyboardShortcut` with `KeyCode`-only handling
  - Replace `UnityInput.Current` with `Input.GetKeyUp()` (available in Unity 2017.4)
  - Remove `ConfigEntryBase` references from custom drawer delegates

- [ ] Update `Utilities/Utils.cs`
  - Remove `ManualLogSource`/`LogLevel` references
  - Simplify or remove `GetWebsite()` (no `BaseUnityPlugin.Info.Location`)
  - Simplify `OpenLog()` to use Common's logger path

- [ ] Keep `ComboBox.cs` and `ImguiUtils.cs` as-is (already BepInEx-free)

- [ ] Create `ConfigPersistence/IniConfigPersistence.cs`
  - Implements `IConfigurationProvider`
  - Reads/writes INI files (one per mod) — familiar to BepInEx users
  - File location: alongside each mod's DLL (via `IFileManager`)
  - Format: `[Section]\n# Description\nKey = Value`
  - Simple parser — no external dependencies needed for net35

- [ ] Add `LICENSE` (LGPL v3) and `README.md`

### Phase 3: Submodule Integration

Back in the main repo.

- [ ] Add `.gitignore` exception
  - `external/` is gitignored; add `!external/LobotomyCorporationMods.ConfigurationManager/`

- [ ] Add git submodule
  - `git submodule add https://github.com/CTristan/LobotomyCorporationMods.ConfigurationManager.git external/LobotomyCorporationMods.ConfigurationManager`
  - Creates `.gitmodules` at repo root

- [ ] Add fork's project to `LobotomyCorporationMods.sln`

- [ ] Update `.github/workflows/ci.yml`
  - Add `submodules: true` to the checkout step
  - Fork builds as part of the solution

- [ ] Add deployment target in `playwright.json`
  - New entry in `deployTargets`: `"LobotomyCorporationMods.ConfigurationManager"` → `"BaseMods/LobotomyCorporationMods.ConfigurationManager"`

### Phase 4: Example Mod Integration

- [ ] Add config registration to `BadLuckProtectionForGifts/Harmony_Patch.cs`
  - Register settings like `BonusPerWork` (float, default 0.01, range 0.001–0.1)
  - Expose as property: `public float BonusPerWork => (float)_bonusPerWork.Get();`

- [ ] Wire configurable value into patch logic
  - In `CreatureEquipmentMakeInfoPatchGetProb.cs`, replace hardcoded value with `Harmony_Patch.Instance.BonusPerWork`

- [ ] Update tests for BadLuckProtectionForGifts
  - Test with default config values
  - Test with modified config values

## Key Files Reference

| File | Role |
|------|------|
| `Common/Implementations/DefaultLocalizedValues.cs` | Pattern to follow for `ConfigurationRegistry` |
| `Common/Implementations/HarmonyPatchBase.cs` | Base class; config registration happens during init |
| `BadLuckProtectionForGifts/Harmony_Patch.cs` | Example mod to integrate first |
| `external/BepInEx.ConfigurationManager-master/ConfigurationManager.Shared/` | Source material for fork |
| `.github/workflows/ci.yml` | CI pipeline needing submodule support |
| `playwright.json` | Deployment targets list |

## BepInEx Replacement Map (Fork)

| BepInEx Type | Replacement |
|---|---|
| `BaseUnityPlugin` | `MonoBehaviour` (boundary) + plain class (core) |
| `[BepInPlugin]` | Removed; `ConfigurationModInfo` from Common |
| `Config.Bind()` | `ConfigurationRegistry.Register()` |
| `ConfigEntryBase` / `ConfigFile` | `IConfigurationEntry` from Common |
| `Chainloader.PluginInfos` | `ConfigurationRegistry.GetAllEntries()` |
| `TomlTypeConverter` | `Convert.ChangeType` + custom string converters |
| `AcceptableValueBase` | Properties on `IConfigurationEntry` |
| `ManualLogSource` | Common's `ILogger` |
| `KeyboardShortcut` | `KeyCode`-only (sufficient for this game) |
| `UnityInput.Current` | `Input.GetKeyUp()` directly |

## Verification

1. **Unit tests**: Run `dotnet test LobotomyCorporationMods.sln` — all Common config tests pass
2. **Build**: `dotnet build LobotomyCorporationMods.sln` — solution builds including submodule project
3. **CI**: Push to branch, verify GitHub Actions passes with submodule checkout
4. **In-game (manual)**:
   - Deploy without ConfigurationManager → mods work with defaults
   - Deploy with ConfigurationManager → F1 opens GUI, settings are editable and persisted to INI files
   - Change a BadLuckProtection setting → verify it takes effect in gameplay

## Risks & Considerations

- **Directory.Build.props cascade**: MSBuild walks upward from `.csproj`. The fork MUST have its own `Directory.Build.props` to prevent the main repo's from applying (different analysis rules, package versions). Verify this blocks correctly.
- **net35 constraints**: No `Lazy<T>`, no `ConcurrentDictionary`, limited LINQ. Config API must use only net35-compatible constructs.
- **INI persistence timing**: Need to decide when to save — on every value change (simple but potentially slow) or on window close / periodic (better perf, risk of data loss). Recommend: save on window close + periodic auto-save.
- **Submodule developer experience**: Developers must `git submodule update --init` after clone. Document this in setup instructions.
- **Fork maintenance**: The fork diverges significantly from upstream. This is intentional — we're not tracking upstream changes. The Subnautica fork confirms this is a common pattern.
- **LGPL v3 compliance**: The fork DLL must be separately replaceable by users. Since it's a standalone Basemod DLL, this is naturally satisfied.
