# HarmonyDebugPanel: Dual-Mode Harmony 1.09 / Harmony 2 Investigation

## Goal

Make HarmonyDebugPanel work **without BepInEx installed**, using only Harmony 1.09 as the baseline. When BepInEx is detected at runtime, dynamically enable Harmony 2 features. The panel should be a single codebase that serves both audiences.

## Assumption

Harmony 1.09 (`0Harmony109.dll`) is **always available** at runtime. It is the baseline.

---

## Current Architecture

### Project: `HarmonyDebugPanel/`

- **Target**: net35
- **Entry point**: `Plugin.cs` — inherits `BepInEx.BaseUnityPlugin`, uses `[BepInPlugin]` attribute
- **NuGet deps**: `BepInEx.Core`, `BepInEx.BaseLib`, `BepInEx.PluginInfoProps`, `BepInEx.Analyzers`, `UnityEngine.Modules`

### File inventory (36 files)

**Core**:
- `Plugin.cs` — BepInEx plugin entry point, Unity lifecycle (`Awake`/`Update`/`OnGUI`), log file management, log generation
- `PluginConfiguration.cs` — BepInEx `ConfigFile.Bind<T>()` wrapper for hotkeys and section toggles
- `PluginConstants.cs` — GUID, name, version strings
- `DiagnosticReportBuilder.cs` — orchestrates all collectors, builds `DiagnosticReport`

**Interfaces** (clean, no BepInEx/Harmony imports):
- `IInfoCollector<T>` — single `Collect()` method
- `IPatchInspectionSource` — `GetPatches()` returns `IEnumerable<PatchInspectionInfo>`
- `IPluginInfoSource` — `GetPlugins()` returns `IEnumerable<BepInExPluginInspectionInfo>`
- `IExpectedPatchSource` — `GetExpectedPatches(IList<string> debugInfo)` returns `IList<ExpectedPatchInfo>`
- `IHarmonyVersionClassifier` — `Classify(IList<AssemblyName> references)` returns `HarmonyVersion`
- `IAssemblyInspectionSource` — `GetAssemblies()` returns `IEnumerable<AssemblyInspectionInfo>`
- `PatchInspectionInfo` — data record (targetType, targetMethod, patchType, owner, patchMethod, assemblyName, assemblyVersion, assemblyReferences)
- `BepInExPluginInspectionInfo` — data record (pluginId, name, version, assembly info)
- `AssemblyInspectionInfo` — data record (name, version, location, references)

**Collectors** (implementations):
- `HarmonyPatchInspectionSource.cs` — **HARMONY 2 DEPENDENCY** — uses `HarmonyLib.Harmony.GetAllPatchedMethods()` and `Harmony.GetPatchInfo()`
- `ChainloaderPluginInfoSource.cs` — **BEPINEX DEPENDENCY** — uses `BepInEx.Bootstrap.Chainloader.PluginInfos`
- `ExpectedPatchSource.cs` — **HARMONY 2 DEPENDENCY in Phase 3 only** — Phases 1 & 2 are pure reflection/regex
- `BepInExPluginCollector.cs` — uses `IPluginInfoSource` interface (already abstracted), default constructor uses `ChainloaderPluginInfoSource`
- `ActivePatchCollector.cs` — uses `IPatchInspectionSource` interface (already abstracted)
- `BaseModCollector.cs` — pure file I/O scanning of `BaseMods/` directory, no BepInEx/Harmony deps
- `HarmonyVersionClassifier.cs` — pure string matching on assembly reference names, no deps
- `AssemblyInfoCollector.cs` — uses `IAssemblyInspectionSource`, no deps
- `AppDomainAssemblyInspectionSource.cs` — `AppDomain.CurrentDomain.GetAssemblies()`, no deps
- `RetargetHarmonyDetector.cs` — assembly name inspection, no deps

**Models** (all pure data, no deps):
- `DiagnosticReport`, `ModInfo`, `PatchInfo`, `AssemblyInfo`, `RetargetHarmonyStatus`
- `ExpectedPatchInfo`, `MissingPatchInfo`
- `HarmonyVersion` (enum: Harmony1, Harmony2, Unknown)
- `ModSource` (enum: BepInExPlugin, Lmm)
- `PatchType` (enum: Prefix, Postfix, Transpiler, Finalizer)

**Formatting/Rendering** (no BepInEx/Harmony deps):
- `DiagnosticLogFormatter.cs` — formats `DiagnosticReport` to text lines
- `DiagnosticOverlay.cs` — Unity `GUI.*`/`GUILayout.*` rendering, hotkeys, scrollable window

---

## Coupling Analysis

### Files with hard BepInEx dependency (3 files)

| File | Dependency | What it uses |
|------|-----------|-------------|
| `Plugin.cs` | `BepInEx.BaseUnityPlugin` | Base class inheritance, `[BepInPlugin]` attribute, `Config` property, `Logger` property |
| `PluginConfiguration.cs` | `BepInEx.Configuration.ConfigFile` | `ConfigFile.Bind<T>()` for config entries |
| `ChainloaderPluginInfoSource.cs` | `BepInEx.Bootstrap.Chainloader` | `Chainloader.PluginInfos` dictionary iteration |

### Files with hard Harmony 2 dependency (2 files)

| File | Dependency | What it uses |
|------|-----------|-------------|
| `HarmonyPatchInspectionSource.cs` | `HarmonyLib.Harmony` | `Harmony.GetAllPatchedMethods()`, `Harmony.GetPatchInfo()`, `Patch.owner`, `Patch.PatchMethod`, `Patches.Prefixes/Postfixes/Transpilers/Finalizers` |
| `ExpectedPatchSource.cs` | `HarmonyLib.Harmony` (Phase 3 only) | Same APIs as above; also `typeof(HarmonyPatch)` in Phase 1 (but with name-based fallback already implemented) |

### Files with NO external dependency (31 files) — work as-is

Everything else: all interfaces, all models, `DiagnosticOverlay`, `DiagnosticLogFormatter`, `DiagnosticReportBuilder` (uses interfaces), `BaseModCollector`, `HarmonyVersionClassifier`, `AssemblyInfoCollector`, `AppDomainAssemblyInspectionSource`, `RetargetHarmonyDetector`, `ActivePatchCollector`, `BepInExPluginCollector` (interface-based).

---

## The Critical Problem: Active Patch Enumeration on Harmony 1.09

### Harmony 2 API (what we use now)

```csharp
// Static methods — get ALL patches from ALL owners globally
foreach (var method in Harmony.GetAllPatchedMethods()) {
    var patchInfo = Harmony.GetPatchInfo(method);
    // patchInfo.Prefixes, .Postfixes, .Transpilers, .Finalizers
    // Each Patch has: .owner, .PatchMethod
}
```

### Harmony 1.09 API (what the mods use)

```csharp
// Instance method — only returns patches from THIS instance
HarmonyInstance harmony = HarmonyInstance.Create("my.mod.id");
harmony.PatchAll(assembly);
// harmony.GetPatchedMethods() — returns only methods patched by THIS instance
```

**There is no public global "get all patches" API in Harmony 1.09.**

### Proposed Solution: Reflect into `Harmony.HarmonySharedState`

Harmony 1.x stores global patch state in an internal static class called `HarmonySharedState`. It contains:
- A method or property to get all patched methods globally
- A method to get patch info for a given method

Since `0Harmony109.dll` is a **fixed, shipped binary that will never change**, the reflection targets are stable.

**This is the #1 unknown that needs local verification.** The task is:

1. Load `RetargetHarmony/lib/0Harmony109.dll` (located at `/RetargetHarmony/lib/0Harmony109.dll`)
2. Use reflection or a decompiler (ILSpy/dnSpy) to inspect the internal types
3. Find `Harmony.HarmonySharedState` (or equivalent)
4. Document: what static methods/fields does it expose? What types do they return?
5. Determine if we can enumerate all patches and their owners via reflection

**Alternative**: If `HarmonySharedState` doesn't expose what we need, we could intercept `HarmonyInstance.Create()` calls to track all instances and then call instance methods on each. But this is more complex.

---

## Proposed Architecture

### Entry Point Refactoring

**Current**: `Plugin : BaseUnityPlugin` (requires BepInEx)

**Proposed**: Split into three parts:
1. **`DiagnosticPanel : MonoBehaviour`** — the core panel (no BepInEx imports). Contains `Awake()`, `Update()`, `OnGUI()`, config management, log generation. This is essentially the current `Plugin.cs` with BepInEx types removed.
2. **`Plugin : BaseUnityPlugin`** (thin BepInEx wrapper) — creates a `GameObject`, does `AddComponent<DiagnosticPanel>()`, passes BepInEx config/logger. Only exists for BepInEx users.
3. **Non-BepInEx entry** — any Harmony 1.09 mod can do `new GameObject("DebugPanel").AddComponent<DiagnosticPanel>()` to load the panel.

### Runtime Capability Detection

```csharp
// In DiagnosticPanel.Awake() or a factory class:
var chainloaderType = Type.GetType("BepInEx.Bootstrap.Chainloader, BepInEx");
bool hasBepInEx = chainloaderType != null;

var harmony2Type = Type.GetType("HarmonyLib.Harmony, 0Harmony");
bool hasHarmony2 = harmony2Type != null;
```

### Collector Wiring Based on Capabilities

```
If hasBepInEx && hasHarmony2:
  - Use ChainloaderPluginInfoSource (existing)
  - Use HarmonyPatchInspectionSource (existing, Harmony 2)
  - Use ExpectedPatchSource (existing, all 3 phases)

If !hasBepInEx (Harmony 1.09 only):
  - Use NullPluginInfoSource (returns empty list — no BepInEx plugins to show)
  - Use Harmony109PatchInspectionSource (NEW — reflects into HarmonySharedState)
  - Use Harmony109ExpectedPatchSource (NEW — Phases 1 & 2 unchanged, Phase 3 uses Harmony 1.09 reflection)
```

### New Files Needed

| File | Purpose |
|------|---------|
| `DiagnosticPanel.cs` | Core MonoBehaviour (extracted from Plugin.cs, no BepInEx) |
| `Harmony109PatchInspectionSource.cs` | Implements `IPatchInspectionSource` using reflection into Harmony 1.09 internals |
| `NullPluginInfoSource.cs` | Implements `IPluginInfoSource`, returns empty list |
| `DiagnosticPanelConfiguration.cs` | INI-style config parser (replaces BepInEx ConfigFile; similar to RetargetHarmony's DebugLogger config parser) |
| `CapabilityDetector.cs` | Probes for BepInEx/Harmony2 at runtime via `Type.GetType()` |
| Modified `DiagnosticReportBuilder.cs` | Accept collector implementations via DI constructor (already has this!) instead of hardcoding in parameterless constructor |

### Files That Need Modification

| File | Change |
|------|--------|
| `Plugin.cs` | Becomes thin BepInEx wrapper that creates `DiagnosticPanel` MonoBehaviour |
| `DiagnosticReportBuilder.cs` | Parameterless constructor uses `CapabilityDetector` to pick implementations |
| `ExpectedPatchSource.cs` | Phase 3 needs to be strategy-based (Harmony 2 or Harmony 1.09 reflection) |

---

## What Already Works Without Changes

These features use zero BepInEx or Harmony 2 APIs:

- In-game GUI overlay rendering (Unity `OnGUI`)
- Hotkey toggle (F9) and manual refresh (F10)
- Auto-refresh timer (60 frames for 60 seconds)
- Loaded assembly inspection (`AppDomain.CurrentDomain.GetAssemblies()`)
- BaseMod/LMM mod detection (directory scanning)
- RetargetHarmony detection (assembly reference name checking)
- Harmony version classification (string matching on assembly names)
- Expected patch detection Phase 1 (reflection on `[HarmonyPatch]` attributes — already has name-based fallback for cross-version compatibility)
- Expected patch detection Phase 2 (regex source file scanning)
- Log formatting (all of `DiagnosticLogFormatter`)
- Log file generation and aggregation
- All data models and interfaces
- Missing patch calculation
- Patch-to-mod correlation

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|-----------|-------|
| Overlay, rendering, formatting | 95% | Pure Unity, zero changes needed |
| Assembly/mod detection | 95% | Already BepInEx-independent |
| Expected patch detection (Phases 1 & 2) | 90% | Already work cross-version |
| Config system replacement | 90% | INI parser pattern exists in DebugLogger |
| Entry point refactor | 85% | Standard MonoBehaviour extraction |
| Dynamic BepInEx detection | 90% | Simple `Type.GetType()` probe |
| **Active patch enumeration (Harmony 1.09)** | **75-80%** | **Depends on HarmonySharedState internals — NEEDS LOCAL VERIFICATION** |

**Overall: ~85% likely to work reliably.** The one real risk is whether Harmony 1.09's `HarmonySharedState` exposes enough for global patch enumeration.

---

## First Step: Local Verification Task

Before any code changes, verify the Harmony 1.09 internal API:

1. **Decompile `RetargetHarmony/lib/0Harmony109.dll`** using ILSpy, dnSpy, or `dotnet-ildasm`
2. **Find the global patch state class** — likely `Harmony.HarmonySharedState` or similar
3. **Document its API surface**: static methods, fields, return types
4. **Specifically look for**:
   - A way to get all patched `MethodBase` objects globally
   - A way to get patch info (prefixes, postfixes, transpilers) for a given method
   - The structure of the patch info objects (do they have `owner` and `PatchMethod`?)
5. **Also check**: `HarmonyInstance` public API — does `GetPatchedMethods()` exist? What does it return?

If `HarmonySharedState` provides what we need, the implementation is straightforward. If not, we need the fallback strategy of intercepting `HarmonyInstance.Create()` calls.

---

## Key Source File References

Files to read for full context:

| File | Path |
|------|------|
| Current entry point | `HarmonyDebugPanel/Plugin.cs` |
| Report builder (orchestration) | `HarmonyDebugPanel/DiagnosticReportBuilder.cs` |
| Harmony 2 patch source (to replicate for H1.09) | `HarmonyDebugPanel/Implementations/Collectors/HarmonyPatchInspectionSource.cs` |
| BepInEx plugin source (to create null version) | `HarmonyDebugPanel/Implementations/Collectors/ChainloaderPluginInfoSource.cs` |
| Expected patch detection (3-phase) | `HarmonyDebugPanel/Implementations/Collectors/ExpectedPatchSource.cs` |
| BepInEx plugin collector (interface-based) | `HarmonyDebugPanel/Implementations/Collectors/BepInExPluginCollector.cs` |
| Existing INI config parser pattern | `RetargetHarmony/DebugLogger.cs` |
| Harmony 1.09 usage in mods | `LobotomyCorporationMods.Common/Implementations/HarmonyPatchBase.cs` |
| Harmony 1.09 DLL | `RetargetHarmony/lib/0Harmony109.dll` |
| Project file | `HarmonyDebugPanel/HarmonyDebugPanel.csproj` |

---

## Project Conventions to Follow

- Every `.cs` file starts with `// SPDX-License-Identifier: MIT`
- `var` preferred, Allman brace style, `#region` wraps `using` directives
- Private fields: `_camelCase`; private static: `s_camelCase`
- Interfaces in `Interfaces/` folder, implementations in `Implementations/`
- `[ExcludeFromCodeCoverage]` on boundary/runtime classes that can't be unit tested
- See `.claude/CLAUDE.md` for full conventions (constructor categories, test conventions, warning suppression rules, etc.)
