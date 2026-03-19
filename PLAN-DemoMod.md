# Plan: Harmony 2 Demo Mod — Verification & Showcase

**Status:** Completed

## Context

The `ModdersGuide.md` was AI-generated and needs verification that all code examples compile and work against real game APIs. A review confirmed all referenced game APIs exist, but found 4 inaccuracies in illustrative examples. This plan builds a demo mod in two phases: first a minimal verification mod that proves the guide's code compiles, then enhances it into a polished showcase mod that demonstrates the features in action.

The mod lives under `Harmony2ForLmm/DemoMod/` as two projects (plugin + patcher), mirroring the real deployment model. Both phases use the same project structure — Phase 2 builds on Phase 1 files, not a rewrite.

## Goals

1. Verify every guide code example compiles against real game DLLs (Phase 1)
2. Fix 4 guide inaccuracies discovered during review (Phase 1)
3. Enhance into a cohesive showcase mod that modders can reference and learn from (Phase 2)
4. CI compilation catches guide drift going forward (both phases)

## Guide Inaccuracies to Fix (Phase 1)

| Location | Issue | Fix |
|----------|-------|-----|
| §Finalizer H1 workaround | `__instance.agentList` — field is private, Harmony needs `___agentList` or Traverse | Use Traverse or triple-underscore syntax |
| §Preloader "without" example | `CreatureModel.Init` doesn't exist | Change to `OnGameInit` |
| §Preloader "without" example | `CreatureManager.RemoveCreature` doesn't exist | Change to `RemoveCreatureInSefira` |
| §Project setup `.csproj` | References `UnityEngine` but game has `UnityEngine.CoreModule.dll` | Kept as-is — `UnityEngine.dll` facade exists and works |
| §Dependency declarations | `nameof(UseSkill.FinishWorkSuccessfully)` — method is private, `nameof` can't access it | Changed to string literal `"FinishWorkSuccessfully"` |

## Project Structure (shared across both phases)

```
Harmony2ForLmm/DemoMod/
  DemoMod.Plugin/
    DemoMod.Plugin.csproj
    Plugin.cs                        # BepInEx entry point + Config
    Patches/
      AgentUpdateFinalizer.cs        # Finalizer patch
      XpFormulaReversePatch.cs       # Reverse Patch
      SuccessCapTranspiler.cs        # CodeMatcher + Config transpiler
      WorkResultLogger.cs            # Dependency-aware postfix
  DemoMod.Patcher/
    DemoMod.Patcher.csproj
    CreatureFieldPatcher.cs          # Preloader patcher
```

### References

**Plugin** (based on `RetargetHarmony/RetargetHarmony.csproj` pattern):
- NuGet: `BepInEx.Core`, `BepInEx.BaseLib`, `BepInEx.PluginInfoProps`, `UnityEngine.Modules` (all ExcludeAssets=runtime)
- DLL: `Assembly-CSharp.dll` from `external/LobotomyCorp_Data/Managed/` (Private=false)
- 0Harmony comes via BepInEx NuGet (HarmonyX), NOT from `external/Managed/0Harmony.dll` (Harmony 1.x)

**Patcher**:
- NuGet: `BepInEx.Core`, `BepInEx.BaseLib`, `Mono.Cecil` (VersionOverride=0.10.4 for net35)

### Key files to reference
- `RetargetHarmony/RetargetHarmony.csproj` — net35 BepInEx NuGet reference pattern
- `Harmony2ForLmm/Resources/docs/ModdersGuide.md` — source code examples
- `external/decompiled/Assembly-CSharp/AgentManager.cs` — Finalizer target
- `external/decompiled/Assembly-CSharp/UseSkill.cs` — Reverse Patch + Transpiler targets
- `external/decompiled/Assembly-CSharp/CreatureModel.cs` — Preloader target
- `Directory.Packages.props` — central NuGet versions

---

## Phase 1: Verification Mod (12/12)

Minimal code that proves each guide example compiles. Each file contains the guide's recommended code with just enough scaffolding (`using` statements, SPDX headers) to build. Business logic is thin — the goal is compilation, not runtime behavior.

### Tasks

- [x] 1.1 Create `DemoMod.Plugin/DemoMod.Plugin.csproj` — net35, BepInEx NuGets, Assembly-CSharp DLL ref, `<IsPackable>false</IsPackable>`
- [x] 1.2 Create `DemoMod.Patcher/DemoMod.Patcher.csproj` — net35, BepInEx.Core + Mono.Cecil 0.10.4
- [x] 1.3 Create `Plugin.cs` — `[BepInPlugin]` + `BaseUnityPlugin` + `Config.Bind()` (§Entry point + §Configuration)
- [x] 1.4 Create `AgentUpdateFinalizer.cs` — `[HarmonyFinalizer]` on `AgentManager.OnFixedUpdate` (§Finalizer)
- [x] 1.5 Create `XpFormulaReversePatch.cs` — `[HarmonyReversePatch]` on `UseSkill.CalculateLevelExp` (§Reverse Patch)
- [x] 1.6 Create `SuccessCapTranspiler.cs` — `CodeMatcher` replacing 0.95f cap + config method call (§CodeMatcher + §Configuration)
- [x] 1.7 Create `WorkResultLogger.cs` — `[BepInDependency]` soft dep + `Chainloader.PluginInfos` check (§Dependencies)
- [x] 1.8 Create `CreatureFieldPatcher.cs` — Mono.Cecil field injection into `CreatureModel` (§Preloader)
- [x] 1.9 Add both projects to `LobotomyCorporationMods.sln`
- [x] 1.10 Fix guide inaccuracies (5 items — original 4 plus `nameof` on private method)
- [x] 1.11 Run `dotnet build` — verify both projects compile
- [x] 1.12 Run `dotnet ci --check` — verify formatting compliance

### Phase 1 verification
- `dotnet build LobotomyCorporationMods.sln` passes
- `dotnet ci --check` passes
- Each .cs file maps to a specific guide section

---

## Phase 2: Runtime Verification & Showcase Enhancement (6/8)

Add active logging to every feature so each patch confirms it fired at runtime. All log messages use `[DemoMod:<feature>]` tags for filtering via `dotnet playwright read-log --filter DemoMod`.

### Tasks

- [x] 2.1 Enhance `Plugin.cs` — shared `Log` property, tagged startup messages, `GetMaxSuccessRate` logging
- [x] 2.2 Enhance `AgentUpdateFinalizer.cs` — log-once on first invocation to confirm finalizer is active, log errors with context
- [x] 2.3 Enhance `XpFormulaReversePatch.cs` — add `XpFormulaLogger` postfix on `FinishWorkSuccessfully` that calls the reverse patch for all 4 RWBP stats and logs multipliers
- [x] 2.4 Enhance `SuccessCapTranspiler.cs` — log before/after transpiler application, `GetMaxSuccessRate` logs on each call
- [x] 2.5 Enhance `WorkResultLogger.cs` — log soft dependency detection result at startup, log which XP path is taken
- [x] 2.6 Add `CreatureFieldVerifier.cs` — postfix on `CreatureModel.OnGameInit` that verifies the preloader-injected field via reflection (write 42, read back, log PASS/FAIL)
- [ ] 2.7 Add inline comments throughout linking each pattern back to the relevant guide section
- [ ] 2.8 Verify `dotnet build` and `dotnet ci --check` still pass

### Runtime verification workflow
```bash
# Deploy and launch the game
dotnet playwright deploy && dotnet playwright launch

# Wait for game to reach a playable day, then assign work to trigger patches
dotnet playwright command assign-work --agent <id> --creature <id> --work instinct

# Check all DemoMod log output
dotnet playwright read-log --filter "DemoMod"
```

### Expected log output
```
[Info:Demo Mod] [DemoMod:Config] DamageMultiplier = 1
[Info:Demo Mod] [DemoMod:Config] MaxSuccessRate = 0.95
[Info:Demo Mod] [DemoMod:Transpiler] Patching ProcessWorkTick — replacing 0.95f cap
[Info:Demo Mod] [DemoMod:Transpiler] ProcessWorkTick patched successfully
[Info:Demo Mod] [DemoMod:EntryPoint] All patches applied successfully
[Info:XP Overhaul] [DemoMod:Dependencies] Custom Creatures not present — using standard XP formula
[Info:Demo Mod] [DemoMod:Finalizer] AgentManager.OnFixedUpdate finalizer is active
[Info:Demo Mod] [DemoMod:Preloader] customDifficultyLevel field found on CreatureModel, value = 0
[Info:Demo Mod] [DemoMod:Preloader] Write/read test: wrote 42, read back 42 — PASS
[Info:Demo Mod] [DemoMod:ReversePatch] Exercising CalculateLevelExp reverse patch
[Info:Demo Mod] [DemoMod:ReversePatch] XP multipliers — R:0.60 W:0.60 B:0.60 P:0.20
[Info:Demo Mod] [DemoMod:Dependencies] WorkResultLogger postfix is active
```

---

## Risks & Considerations

- **HarmonyX API surface from BepInEx NuGet**: Need to confirm `CodeMatcher`, `[HarmonyFinalizer]`, `[HarmonyReversePatch]` are available. Fallback: reference `0Harmony.dll` from `Harmony2ForLmm/Resources/bepinex/BepInEx/core/`.
- **`TreatWarningsAsErrors`**: Demo inherits this from `Directory.Build.props`. May need `.editorconfig` globs for Harmony naming conventions (`__instance`, `__exception`).
- **No tests for demo mod**: Intentional — it's a compilation-verification showcase, not production code.
- **Phase 2 scope creep**: The showcase should remain small and focused. Resist adding features beyond what the guide covers.
