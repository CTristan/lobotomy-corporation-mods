# Plan: DemoMod Showcase Expansion

**Status:** Completed

## Context

The DemoMod at `Harmony2ForLmm/DemoMod/` was expanded from 7 features to a full showcase of 11 Harmony 2 + BepInEx features, including 4 new features not previously in the guide: priority/ordering, manual patching, config validation, and MonoBehaviour lifecycle.

## Goals

1. Add 4 new features to both the Modder's Guide and DemoMod
2. Polish all files with inline comments, a README, and updated testing docs
3. Prepare the DemoMod source for distribution as an embedded zip in the installer
4. Installer extraction button deferred to a future phase

## Tasks

### Phase A: New DemoMod Features

- [x] A1. Create `Patches/CreatureUpdatePriorityDemo.cs` — Priority & ordering demo
- [x] A2. Create `Patches/EnergyMultiplierManualPatch.cs` — Manual patching demo
- [x] A3. Modify `Plugin.cs` — Config validation + MonoBehaviour lifecycle
- [x] A4. Verify `dotnet build` and `dotnet ci --check` pass

### Phase B: Modder's Guide Expansion

- [x] B1. Add "Patch Priority & Ordering" section after Reverse Patch
- [x] B2. Add "Manual Patching" section after CodeMatcher
- [x] B3. Add "Config Validation" subsection within Configuration
- [x] B4. Add "MonoBehaviour Lifecycle" section after BepInEx plugin entry point
- [x] B5. Update quickstart feature table at top of guide to include 4 new features

### Phase C: Polish

- [x] C1. Add inline `// Guide: §Section` comments to all DemoMod files
- [x] C2. Create `Harmony2ForLmm/DemoMod/README.md`
- [x] C3. Update `Harmony2ForLmm/DemoMod/TESTING.md` with verification steps for 4 new features
- [x] C4. Final `dotnet build` + `dotnet ci --check` verification

### Phase D: Distribution Prep

- [x] D1. Add MSBuild pre-build target to zip DemoMod source into `Resources/DemoMod.zip`
- [x] D2. Add Content include for DemoMod.zip in installer csproj
- [x] D3. Verify installer builds with embedded zip
- [x] D4. Add TODO comment in installer for future "Extract Sample Project" button

### Phase E: Bookkeeping

- [x] E1. Update `PLAN-DemoMod.md` status to Completed
- [x] E2. Create `PLAN-DemoModShowcase.md` at repo root
- [x] E3. Update `PLANS.md` with new plan entry

## New Files

| File | Purpose |
|------|---------|
| `DemoMod.Mod/Patches/CreatureUpdatePriorityDemo.cs` | Priority/ordering demo |
| `DemoMod.Mod/Patches/EnergyMultiplierManualPatch.cs` | Manual patching demo |
| `Harmony2ForLmm/DemoMod/README.md` | Build/deploy instructions |

## Modified Files

| File | Changes |
|------|---------|
| `DemoMod.Mod/Plugin.cs` | Lifecycle methods, config validation, manual patch orchestration |
| `DemoMod.Mod/DemoMod.Mod.csproj` | IMGUIModule reference |
| `Harmony2ForLmm/Resources/docs/ModdersGuide.md` | 4 new sections + quickstart table |
| `Harmony2ForLmm/DemoMod/TESTING.md` | Verification steps for new features |
| `Harmony2ForLmm/Harmony2ForLmm.csproj` | Zip pre-build target + Content include |
| All existing DemoMod `.cs` files | Inline guide-section comments |
