# Demo Mod Testing Guide

Instructions for verifying every Harmony 2 feature demonstrated by the demo mod using LobotomyPlaywright.

## Prerequisites

- Game installed and `dotnet playwright find-game` configured (config.json exists)
- Vanilla game snapshot available at `external/snapshots/LobotomyCorp_vanilla/`
- `dotnet tool restore && dotnet setup` completed (game DLLs in `external/`)
- Solution builds cleanly: `dotnet build LobotomyCorporationMods.sln`

## Deploy

The `demo` profile installs LMM + BepInEx + RetargetHarmony + Playwright + both demo mod DLLs:

```bash
dotnet playwright deploy --profile demo
```

This deploys:
- `DemoMod.Plugin.dll` → `BaseMods/DemoMod.Plugin/` (BepInEx plugin with all patches)
- `DemoMod.Patcher.dll` → `BepInEx/patchers/DemoMod.Patcher/` (preloader patcher)
- `RetargetHarmony.dll` → `BepInEx/patchers/RetargetHarmony/` (required for Harmony 2 under LMM)
- `Hemocode.Playwright.dll` → `BaseMods/Hemocode.Playwright/` (TCP query/command bridge)

## Launch and wait for gameplay

```bash
dotnet playwright launch
```

Wait for the game to fully load. The game starts at the title screen — you need to get to a playable day where agents and creatures exist. Either:
- Load an existing save that has agents deployed to departments
- Start a new game and advance past the tutorial until Day 1 begins

To check if the game is in a playable state:

```bash
dotnet playwright query game
```

Look for `"gameState": "PLAYING"` or similar. Then verify agents and creatures exist:

```bash
dotnet playwright query agents
dotnet playwright query creatures
```

You need at least one agent and one creature to test work-related patches.

## Verify features

### Phase 1: Startup verification (no interaction needed)

After the game loads, check logs for startup messages:

```bash
dotnet playwright read-log --filter "DemoMod"
```

**Expected startup log lines (in approximate order):**

| Tag | Expected message | Feature verified |
|-----|-----------------|------------------|
| `[DemoMod:Config]` | `DamageMultiplier = 1` | Config system reads defaults |
| `[DemoMod:Config]` | `MaxSuccessRate = 0.95` | Config system reads defaults |
| `[DemoMod:Transpiler]` | `Patching ProcessWorkTick — replacing 0.95f cap` | CodeMatcher found the IL pattern |
| `[DemoMod:Transpiler]` | `ProcessWorkTick patched successfully` | CodeMatcher completed without error |
| `[DemoMod:EntryPoint]` | `All patches applied successfully` | BepInEx plugin loaded, Harmony.PatchAll() succeeded |
| `[DemoMod:Dependencies]` | `Custom Creatures not present — using standard XP formula` | Soft dependency detection works |

If `[DemoMod:EntryPoint]` appears, the BepInEx plugin entry point and Harmony patching both work.

If `[DemoMod:Transpiler]` messages appear, the CodeMatcher successfully found and replaced the 0.95f constants in the IL.

### Phase 2: Tick-based verification (wait for game tick)

Once gameplay starts and agents exist, the finalizer fires on the first agent manager tick:

```bash
dotnet playwright read-log --filter "Finalizer"
```

**Expected:**

| Tag | Expected message | Feature verified |
|-----|-----------------|------------------|
| `[DemoMod:Finalizer]` | `AgentManager.OnFixedUpdate finalizer is active` | Finalizer patch registered and running |

This confirms the `[HarmonyFinalizer]` attribute works and the patch fires on every `AgentManager.OnFixedUpdate` call (logs once to avoid spam).

### Phase 3: Creature initialization verification

When any creature initializes (happens on day load), the preloader field verifier fires:

```bash
dotnet playwright read-log --filter "Preloader"
```

**Expected (if DemoMod.Patcher is installed in `BepInEx/patchers/`):**

| Tag | Expected message | Feature verified |
|-----|-----------------|------------------|
| `[DemoMod:Preloader]` | `customDifficultyLevel field found on CreatureModel, value = 0` | Preloader patcher injected the field via Mono.Cecil |
| `[DemoMod:Preloader]` | `Write/read test: wrote 42, read back 42 — PASS` | Injected field is functional at runtime |

**Expected (if DemoMod.Patcher is NOT installed):**

| Tag | Expected message | Meaning |
|-----|-----------------|---------|
| `[DemoMod:Preloader]` | `customDifficultyLevel field NOT found` | Patcher DLL missing from `BepInEx/patchers/` |

### Phase 4: Work assignment verification (requires player action)

Assign work to trigger the reverse patch and dependency-aware postfix:

```bash
# Get available agent and creature IDs
dotnet playwright query agents
dotnet playwright query creatures

# Assign work (substitute real IDs)
dotnet playwright command assign-work --agent <agent_id> --creature <creature_id> --work instinct
```

Wait for the work to complete (this takes several seconds of game time). Speed up if needed:

```bash
dotnet playwright command set-game-speed --speed 3
```

After work completes, check for reverse patch and dependency logs:

```bash
dotnet playwright read-log --filter "ReversePatch"
dotnet playwright read-log --filter "Dependencies"
```

**Expected after successful work completion:**

| Tag | Expected message | Feature verified |
|-----|-----------------|------------------|
| `[DemoMod:ReversePatch]` | `Exercising CalculateLevelExp reverse patch` | Reverse patch stub was replaced with real method |
| `[DemoMod:ReversePatch]` | `XP multipliers — R:X.XX W:X.XX B:X.XX P:X.XX` | Reverse patch returns real game values |
| `[DemoMod:Dependencies]` | `WorkResultLogger postfix is active` | Dependency-aware postfix is running |
| `[DemoMod:Dependencies]` | `Using standard XP path` | Soft dependency branching works (debug level) |

The XP multiplier values will vary based on the agent's stats and creature's risk level. Any non-zero values confirm the reverse patch correctly invoked the private `CalculateLevelExp` method.

**Expected during work ticks (debug level):**

| Tag | Expected message | Feature verified |
|-----|-----------------|------------------|
| `[DemoMod:Transpiler]` | `GetMaxSuccessRate() called, returning 0.95` | Config+Transpiler pipeline works at runtime |

Note: `GetMaxSuccessRate` logs at Debug level, which may be filtered out depending on BepInEx log level config. If not visible, check `BepInEx/config/BepInEx.cfg` and set `[Logging.Console]` / `[Logging.Disk]` LogLevel to include `Debug`.

## Full verification checklist

Run this after all phases to get a complete picture:

```bash
dotnet playwright read-log --filter "DemoMod"
```

All 6 features should have at least one `[DemoMod:*]` tagged log line:

- [ ] `[DemoMod:EntryPoint]` — BepInEx plugin loaded
- [ ] `[DemoMod:Config]` — Config values read from .cfg file
- [ ] `[DemoMod:Finalizer]` — Finalizer is active on AgentManager.OnFixedUpdate
- [ ] `[DemoMod:Transpiler]` — CodeMatcher patched ProcessWorkTick successfully
- [ ] `[DemoMod:ReversePatch]` — CalculateLevelExp called via reverse patch with real values
- [ ] `[DemoMod:Dependencies]` — Soft dependency detected/not-detected, postfix active
- [ ] `[DemoMod:Preloader]` — Field injected and read/write test PASS

## Automated flow (no human interaction)

The complete testing sequence can be executed without manual intervention using the title menu commands:

```bash
# Deploy demo profile
dotnet playwright deploy --profile demo

# Launch game and wait for title screen
dotnet playwright launch

# Navigate from title screen into gameplay (requires existing save)
dotnet playwright command continue

# Wait for gameplay to begin (scene load + day start)
dotnet playwright wait event OnStageStart --timeout 60

# Verify game is in playable state
dotnet playwright query game           # expect "gameState": "PLAYING" or similar
dotnet playwright query agents         # verify agents exist
dotnet playwright query creatures      # verify creatures exist

# Phase 1-3: Check startup, finalizer, and preloader logs
dotnet playwright read-log --filter "DemoMod"

# Phase 4: Assign work (substitute real IDs from query results)
dotnet playwright command assign-work --agent <agent_id> --creature <creature_id> --work instinct

# Wait for work to complete
dotnet playwright wait event OnWorkCoolTimeEnd --timeout 120

# Verify all 7 feature tags in logs
dotnet playwright read-log --filter "DemoMod"

# Cleanup
dotnet playwright stop
```

**Prerequisites for automated flow:**
- A save file must already exist with agents deployed to departments
- Use `dotnet playwright query titlemenu` before `command continue` to verify `hasSaveData: true`
- If no save exists, use `dotnet playwright command new-game` instead (but you'll need to advance past the tutorial manually)

## Cleanup

```bash
dotnet playwright stop
```

To restore the game to vanilla state for other testing:

```bash
dotnet playwright deploy --profile vanilla
```

## Troubleshooting

**No `[DemoMod:*]` log lines at all:**
- Check `dotnet playwright read-log --tail 50` for BepInEx startup errors
- Look for `TypeLoadException` or `FileNotFoundException` mentioning DemoMod
- Verify DLLs exist: check `BaseMods/DemoMod.Plugin/DemoMod.Plugin.dll` and `BepInEx/patchers/DemoMod.Patcher/DemoMod.Patcher.dll` in the game directory

**`[DemoMod:EntryPoint]` appears but no patch messages:**
- `PatchAll()` may have thrown — look for Harmony errors: `dotnet playwright read-log --filter "Error"`
- Game may have updated and method signatures changed — check for `MissingMethodException`

**`[DemoMod:Preloader]` says field NOT found:**
- `DemoMod.Patcher.dll` must be in `BepInEx/patchers/`, not `BaseMods/`
- Verify the `demo` profile was used: `dotnet playwright deploy --profile demo`

**`[DemoMod:ReversePatch]` shows an error:**
- The reverse patch may fail if `UseSkill.CalculateLevelExp` signature changed
- Check the error message for details — common cause is game update changing private method internals

**Work assignment fails:**
- Agent must be alive and idle (not already working, not panicking, not dead)
- Creature must be in a department with the agent deployed there
- Use `dotnet playwright query agents <id>` to check agent state
