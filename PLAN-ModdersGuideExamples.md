# Plan: Modder's Guide Real Code Examples

**Status**: Completed
**Created**: 2025-03-18

## Context

The Modder's Guide (`Harmony2ForLmm/Resources/docs/ModdersGuide.md`, 759 lines) covers BepInEx and Harmony 2 features with generic examples. The audience — modders familiar with LMM/Harmony 1 — would benefit from real game code examples showing *why* each H2/BepInEx feature is better than what they already have.

This plan is an annotated target list produced by Phase 1 (codebase survey). It serves as input for future phases that write actual guide content.

## Quality Criteria

| Criterion | Description |
|-----------|-------------|
| **Recognizable** | Modders know this class/method from gameplay |
| **Concise** | Method is under ~50 lines, showable in the guide |
| **Clear H2 advantage** | Concrete "you can't do this with H1" narrative |
| **Realistic scenario** | Something real modders actually want to do |
| **Self-contained** | Doesn't require deep knowledge of many other classes |

---

## Feature 1: Finalizer (H2-only)

**H1 limitation**: Postfix doesn't run if the original method throws. Modders must wrap the entire original call with a Prefix returning `false`.

### Recommended Target

**`AgentManager.OnFixedUpdate()`** — `external/decompiled/Assembly-CSharp/AgentManager.cs`, lines 337–348

```csharp
public void OnFixedUpdate()
{
    foreach (AgentModel agent in agentList)
    {
        try
        {
            agent.OnFixedUpdate();
        }
        catch (Exception)
        {
        }
    }
}
```

**Why this target**: Completely silent exception swallowing — the `catch (Exception) { }` block discards all errors with zero logging. A Finalizer can intercept these exceptions, log them properly, and still allow cleanup to proceed. The method is only 12 lines, instantly recognizable (agents update every tick), and the problem is obvious at a glance.

**Modding scenario**: A modder adding per-agent stat tracking needs their cleanup code to run even when a specific agent's update throws. With H1, they'd need a Prefix that replaces the entire method. With H2, a Finalizer catches the exception, logs it, and lets their code run.

### Alternate Candidates

| Method | File | Lines | Notes |
|--------|------|-------|-------|
| `CreatureManager.OnFixedUpdate()` | `CreatureManager.cs` | 587–607 | Nested try-catch: inner per-creature, outer for `InvalidOperationException`. Logs via `Debug.LogError(message)` with no creature context. Slightly more complex than AgentManager. |
| `OfficerManager.OnFixedUpdate()` | `OfficerManager.cs` | 137–149 | Same silent `catch (Exception) { }` pattern as AgentManager. Less familiar class to modders. |
| `RabbitManager.OnFixedUpdate()` | `RabbitManager.cs` | 65–114 | Two separate silent try-catch blocks. More complex (50 lines). |
| `SefiraBossManager.Init()` | `SefiraBossManager.cs` | 121–163 | Mixed: `FileReadException` catch with minimal context + silent `catch (Exception) { }`. Good for showing Finalizer on initialization code. |
| `OrdealManager.ActivateOrdeal()` | `OrdealManager.cs` | 131–151 | `catch (Exception message) { Debug.LogError(message); return false; }`. Good "swallowed return" example. |

---

## Feature 2: Reverse Patch (H2-only)

**H1 limitation**: To call a private method, modders must copy-paste the logic or use reflection at runtime.

### Recommended Target

**`UseSkill.CalculateLevelExp(RwbpType)`** — `external/decompiled/Assembly-CSharp/UseSkill.cs`, lines 385–443

```csharp
private float CalculateLevelExp(RwbpType rwbpType)
{
    // Gets agent stat level for the given RWBP type
    // Computes multiplier based on (statLevel - creatureRiskLevel) difference:
    //   -3 → 1.4f, -2 → 1.2f, -1/0 → 1.0f, +1 → 0.8f, ... +4 → 0.2f
    // Applies base exp rate from array {0.6, 0.55, 0.5, 0.45, 0.4}
    // Battle stat (P) gets additional /3f penalty
    // Returns final float multiplier
}
```

**Why this target**: Pure calculation, returns a `float`, no side effects, self-contained. The level-difference multiplier table and base exp array are exactly what modders want to query. It's private, so H1 modders must use `AccessTools.Method` + `Invoke` or copy the 58 lines. A Reverse Patch makes it callable as a regular method.

**Modding scenario**: A "training optimizer" mod that previews XP gains before assigning work. The mod needs the exact game formula, not a copy that might drift from patches.

### Alternate Candidates

| Method | File | Lines | Notes |
|--------|------|-------|-------|
| `AgentModel.UpdateBestRwbp()` | `AgentModel.cs` | 1072–1094 | Private. Determines agent's best stat for panic type selection. Self-contained, 22 lines. Good secondary example. |
| `UseSkill.CalculateDmgExp(float)` | `UseSkill.cs` | 360–383 | Private. Maps damage ratio to XP multiplier via 6-tier threshold chain. Very concise (23 lines). |
| `UseSkill.ProcessWorkTick(out bool)` | `UseSkill.cs` | 536–601 | Private. Full work cube roll logic. Too long (65 lines) and has side effects — better as Transpiler target. |
| `AgentModel.TakeMentalCalculate(float)` | `AgentModel.cs` | 2149–2160 | Private. Mental damage severity calculation. Appears possibly orphaned — verify before using. |

---

## Feature 3: Transpiler (CodeMatcher API)

**H1 limitation**: Transpilers existed in H1 but lacked CodeMatcher. H2's CodeMatcher makes IL manipulation safer and more readable.

### Recommended Target

**`UseSkill.ProcessWorkTick(out bool)` — the `0.95f` success cap** — `external/decompiled/Assembly-CSharp/UseSkill.cs`, lines 553–555

```csharp
// Line 553-555 within ProcessWorkTick:
if (workSuccessProb > 0.95f)
{
    workSuccessProb = 0.95f;
}
```

**Why this target**: The most famous constant in LC modding. The 95% work success cap is mid-method (line 553 of a method starting at line 536), after probability bonuses are calculated but before the random roll. Prefix can't change it (fires too early, before bonuses), Postfix can't change it (fires too late, after the roll). Only a Transpiler can surgically replace this constant. CodeMatcher makes finding and replacing the `0.95f` IL operand straightforward.

**Modding scenario**: A difficulty mod wants to raise or lower the max success rate. This pairs naturally with Feature 4 (ConfigEntry) to make it player-configurable.

### Alternate Candidates

| Method | File | Lines | Notes |
|--------|------|-------|-------|
| `UseSkill.FinishWorkSuccessfully()` | `UseSkill.cs` | 505 | Justice XP `1.5f` multiplier. Mid-method local variable. Niche but clean. |
| `UseSkill.CalculateDmgExp(float)` | `UseSkill.cs` | 360–383 | Six hardcoded threshold/return pairs. Good for showing CodeMatcher on a chain. |
| `AgentModel.CalculateStatLevel()` | `AgentModel.cs` | 477–496 | Stat thresholds: 30, 45, 65, 85 for levels 1–5. Clean threshold chain. |
| `AgentModel.HorrorDamage()` | `AgentModel.cs` | 2305–2348 | Mental damage percentages by encounter level: 10%, 30%, 60%, 100%. |
| `DefenseInfo.GetDefenseType()` | `DefenseInfo.cs` | 33–56 | Defense tier boundaries: 0.5, 1.0, 1.5. Lower gameplay impact. |

---

## Feature 4: BepInEx ConfigEntry

**Approach**: Pair with the Transpiler example to make the `0.95f` success cap player-configurable.

### Recommended Target

**Config + Transpiler combo**: `ConfigEntry<float>` for the work success cap.

```csharp
// In BepInEx plugin class:
public static ConfigEntry<float> MaxSuccessRate;

void Awake()
{
    MaxSuccessRate = Config.Bind("Difficulty", "MaxSuccessRate", 0.95f,
        "Maximum work success probability (0.0 to 1.0)");
}

// Transpiler reads MaxSuccessRate.Value instead of hardcoded 0.95f
```

**Why this pairing**: Shows the natural Config → Transpiler pipeline. The config file is human-editable, hot-reloadable, and self-documenting. This is the most common pattern modders will use.

**Modding scenario**: Players can set difficulty by editing `BepInEx/config/com.example.difficultymod.cfg`.

### Alternate Config Candidates

| Setting | Source | Notes |
|---------|--------|-------|
| `SefiraPanel.maxAgentCount` (const `5`) | `SefiraPanel.cs:205` | Agent count per department. Would need Preloader to change the const, or Config + Transpiler for methods that read it. |
| XP multipliers from `CalculateLevelExp` | `UseSkill.cs` | Multiple configurable values (level-diff table, base rates). |
| Ordeal timing thresholds | Various ordeal classes | Less recognizable to new modders. |

---

## Feature 5: BepInEx Logging

**H1 limitation**: Only `Debug.Log`/`Debug.LogError` — no log levels, no source tagging, no file output.

### Recommended Target

**`CreatureManager.OnFixedUpdate()` catch block** — `external/decompiled/Assembly-CSharp/CreatureManager.cs`, line 599

```csharp
catch (Exception message)
{
    Debug.LogError(message);  // No creature ID, no method context, no log level
}
```

**Why this target**: Before/after comparison is immediately compelling. The `Debug.LogError(message)` gives a stack trace but no creature context — you can't tell *which* creature caused the error without reading the trace. BepInEx logging adds source tag, log level, file output, and structured context in one line.

**Modding scenario**: Showing the BepInEx equivalent: `Logger.LogError($"Creature {creature.metaInfo.name} (ID: {creature.instanceId}) threw in OnFixedUpdate: {message}");`

### Additional Poor Logging Examples

| Location | Code | Problem |
|----------|------|---------|
| `AgentManager.OnFixedUpdate()` catch | `catch (Exception) { }` | Completely silent — no logging at all |
| `SefiraBossManager.Init()` | `Debug.LogError("Reading file failure " + ex.fileName)` | No boss context, no stack trace |
| `SefiraBossManager.ResetYesodBossSetting()` | `Debug.Log(message)` | Uses `Log` instead of `LogError` for an exception |
| `MagicalGirl_2.cs` | `Debug.Log("is dead!!!!!!!!!!!!!!!!!")` | Unparseable, no context |

---

## Feature 6: BepInEx Dependency Declarations

**Approach**: Architectural example — no single decompiled method to target. Design a realistic multi-mod scenario.

### Recommended Scenario

**Mod A** patches `UseSkill.FinishWorkSuccessfully()` to change XP formulas. **Mod B** (optional) adds custom creatures with non-standard risk levels. Mod A soft-depends on Mod B:

```csharp
[BepInPlugin("com.example.xpoverhaul", "XP Overhaul", "1.0.0")]
[BepInDependency("com.example.customcreatures", BepInDependency.DependencyFlags.SoftDependency)]
public class XpOverhaulPlugin : BaseUnityPlugin
{
    void Awake()
    {
        if (Chainloader.PluginInfos.ContainsKey("com.example.customcreatures"))
        {
            // Adjust XP formula for custom creature risk levels
        }
    }
}
```

**Why this scenario**: Shows both hard and soft dependency patterns. Uses `UseSkill` (already familiar from Features 2–4), so the reader builds on prior examples. The "custom creatures" angle is a real modding desire.

---

## Feature 7: Preloader Patchers (Mono.Cecil)

**H1 limitation**: Harmony operates at runtime via method hooking — it cannot add fields, properties, or interfaces to types.

### Recommended Target

**Adding a field to `CreatureModel`** for custom mod data.

- `CreatureModel` is the core creature data class — every creature instance in the game is one
- Modders often need per-creature state (custom difficulty, tracking data, mod-specific flags)
- Without a Preloader, they must use external `Dictionary<int, CustomData>` keyed by instance ID — fragile and not garbage-collected with the creature

**Reference implementation**: `RetargetHarmony.cs` in this repo is a real working preloader patcher that modders can study.

**Modding scenario**: A difficulty mod adds `public int customDifficultyLevel` to `CreatureModel` via Cecil before the assembly loads.

### Alternate Preloader Candidates

| Target | File | Notes |
|--------|------|-------|
| `SefiraPanel.maxAgentCount` (const `5`) | `SefiraPanel.cs:205` | Change from `const` to `static` field so it's moddable at runtime. Shows a different Cecil operation (field modification vs addition). |
| Adding an interface to `AgentModel` | `AgentModel.cs` | Make `AgentModel` implement a custom interface for type-safe mod interop. Advanced example. |

---

## Diversity Check

| Feature | Primary Class | Notes |
|---------|--------------|-------|
| 1. Finalizer | `AgentManager` | Manager class, update loop |
| 2. Reverse Patch | `UseSkill` | Work/XP calculation |
| 3. Transpiler | `UseSkill` | Same class, different method — the 0.95f cap is too iconic to skip |
| 4. ConfigEntry | (paired with #3) | Config pattern, not class-specific |
| 5. Logging | `CreatureManager` | Manager class, error handling |
| 6. Dependencies | (architectural) | Multi-mod scenario |
| 7. Preloader | `CreatureModel` | Core data model |

`UseSkill` appears in Features 2 and 3, but they target different methods (`CalculateLevelExp` vs `ProcessWorkTick`) with completely different narratives (calling private methods vs replacing constants). The rest are spread across 4 different classes plus 2 architectural examples.

---

## Phases

### Phase 1 — Target Identification (this document) ✅

- [x] Systematic search for Finalizer candidates (exception-handling patterns)
- [x] Systematic search for Reverse Patch candidates (useful private methods)
- [x] Systematic search for Transpiler candidates (hardcoded mid-method constants)
- [x] Identify BepInEx ConfigEntry pairing with Transpiler target
- [x] Collect Debug.Log examples for BepInEx Logging before/after
- [x] Design dependency declaration scenario
- [x] Identify Preloader Patcher target (type-level changes)
- [x] Write finalized annotated target list into this plan file
- [x] Add entry to `PLANS.md`

### Phase 2 — Finalizer (`AgentManager.OnFixedUpdate`) ✅

- [x] Write cleaned decompiled code excerpt
- [x] Write "H1 workaround" showing the old approach
- [x] Write H2 Finalizer solution with full working code
- [x] Integrate into `ModdersGuide.md` (replace/supplement existing generic Finalizer example)
- [x] Review for accuracy against game behavior

### Phase 3 — Reverse Patch (`UseSkill.CalculateLevelExp`) ✅

- [x] Write cleaned decompiled code excerpt
- [x] Write "H1 workaround" showing the old approach (reflection / copy-paste)
- [x] Write H2 Reverse Patch solution with full working code
- [x] Integrate into `ModdersGuide.md` (replace/supplement existing generic Reverse Patch example)
- [x] Review for accuracy against game behavior

### Phase 4 — Transpiler + ConfigEntry (`UseSkill.ProcessWorkTick` 0.95f cap) ✅

- [x] Write cleaned decompiled code excerpt for `ProcessWorkTick`
- [x] Write "H1 workaround" showing the old approach (manual IL or full method replacement)
- [x] Write H2 Transpiler solution using CodeMatcher API
- [x] Write `ConfigEntry<float>` binding for the success cap value
- [x] Integrate Transpiler into `ModdersGuide.md` (add CodeMatcher example alongside existing manual IL example)
- [x] Integrate ConfigEntry into `ModdersGuide.md` (replace/supplement existing generic config example)
- [x] Review for accuracy against game behavior

### Phase 5 — BepInEx Logging (`CreatureManager.OnFixedUpdate`) ✅

- [x] Write cleaned decompiled code excerpt showing `Debug.LogError(message)`
- [x] Write before/after comparison (Unity `Debug.Log` vs BepInEx `Logger`)
- [x] Integrate into `ModdersGuide.md` (supplement existing logging section with real example)
- [x] Review for accuracy against game behavior

### Phase 6 — Dependency Declarations (architectural example) ✅

- [x] Write the multi-mod scenario (XP Overhaul + Custom Creatures)
- [x] Show hard dependency and soft dependency patterns with `Chainloader` check
- [x] Integrate into `ModdersGuide.md` (replace/supplement existing dependency example)
- [x] Review for accuracy

### Phase 7 — Preloader Patcher (`CreatureModel` field addition) ✅

- [x] Write Mono.Cecil example adding a field to `CreatureModel`
- [x] Write "without Preloader" workaround showing `Dictionary<int, CustomData>` approach
- [x] Reference `RetargetHarmony.cs` as a real working preloader patcher
- [x] Integrate into `ModdersGuide.md` (replace/supplement existing generic Preloader example)
- [x] Review for accuracy

### Phase 8 — Final Review

- [x] Read full guide for consistency of tone, format, and detail level
- [x] Trim generic examples that are now redundant (replaced by real game examples)
- [x] Fix duplicated debugging section (appears in both Part 1 and end of guide)
- [x] Review total guide length and adjust if needed

## Risks & Considerations

- **UseSkill appears twice** (Phases 3 & 4) — different methods with different narratives, but may want to swap Phase 3 to `AgentModel.UpdateBestRwbp()` if it feels repetitive in practice
- **Decompiled variable names** may need light cleanup for readability (e.g., `num`, `num2`, `num3`)
- **Method line numbers are hints only** — they shift if the decompiled source is regenerated; use method names as primary identifiers
- **Guide length**: 7 real examples with before/after comparisons will significantly grow the guide — Phase 8 should trim generic examples to compensate
