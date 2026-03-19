# Modder's Guide

## Quickstart: differences from normal LMM mods

If you've written Lobotomy Corporation mods using the standard Basemod/LMM approach,
here's what changes with Harmony 2:

| | LMM (Harmony 1.x) | Harmony 2 for LMM |
|---|---|---|
| **Entry point** | `Basemod_Add_On` class | `BaseUnityPlugin` class with `[BepInPlugin]` |
| **Patching API** | `HarmonyInstance.Create()` | `new Harmony()` |
| **Patch attributes** | `[HarmonyPatch]` (Harmony 1.x) | `[HarmonyPatch]` (Harmony 2 — same syntax, more features) |
| **Mod location** | `BaseMods/` | `BaseMods/` (same!) |
| **Patch types** | Prefix, Postfix, Transpiler | Prefix, Postfix, Transpiler, **Finalizer**, **Reverse Patch** |

Your mod DLL still goes in `BaseMods/`. RetargetHarmony ensures the game loads it correctly
regardless of which Harmony version it targets.

### What you get with Harmony 2 / BepInEx

| Feature | What it does | Section |
|---------|-------------|---------|
| **Finalizer patches** | Run code after a method even if it threw — log, suppress, or replace exceptions without replacing the original method | [Finalizer](#finalizer) |
| **Reverse patches** | Call private game methods directly — type-safe, no reflection, stays in sync with other mods' patches | [Reverse Patch](#reverse-patch) |
| **CodeMatcher** | Find and modify IL patterns with a fluent API — built-in error reporting when patterns break after game updates | [CodeMatcher](#codematcher--pattern-matching-for-transpilers) |
| **BepInEx logging** | Every log line tagged with your mod's name + severity level — no more anonymous `Debug.LogError` noise | [Logging](#logging) |
| **Built-in config system** | Expose game constants as player-editable settings in auto-generated `.cfg` files — no rebuild needed | [Configuration](#configuration) |
| **Dependency declarations** | Control mod load order with hard/soft dependencies — detect optional mods and adapt at runtime | [Dependencies](#dependency-declarations) |
| **Preloader patchers** | Modify assemblies before the game loads them — add fields, change constants, inject types via Mono.Cecil | [Preloader patchers](#preloader-patchers) |

---

## Project setup

### Requirements

- **.NET Framework 3.5** — the game runs on Unity 2017.4 (Mono)
- **BepInEx 5.4.23.5** — reference `BepInEx.Core` NuGet package for plugin APIs
- **HarmonyX** — reference `0Harmony.dll` from BepInEx for Harmony 2 APIs

> **Build note:** To compile against .NET Framework 3.5 on any platform, add the
> `Microsoft.NETFramework.ReferenceAssemblies.net35` NuGet package to your project.
> This provides the reference assemblies without requiring a framework install.

### Minimal project structure

```
MyMod/
  MyMod.csproj
  Plugin.cs           # BepInEx plugin entry point
  Patches/
    SomeClassPatch.cs # Harmony patches
```

### Example `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net35</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>path/to/game/LobotomyCorp_Data/Managed/Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine">
      <HintPath>path/to/game/LobotomyCorp_Data/Managed/UnityEngine.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="0Harmony">
      <HintPath>path/to/game/BepInEx/core/0Harmony.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="BepInEx">
      <HintPath>path/to/game/BepInEx/core/BepInEx.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
```

Set `<Private>false</Private>` on game and BepInEx references so they are not copied
to your output — the game already has them.

---

## BepInEx plugin entry point

```csharp
using BepInEx;
using HarmonyLib;

[BepInPlugin("com.yourname.mymod", "My Mod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        var harmony = new Harmony("com.yourname.mymod");
        harmony.PatchAll();
        Logger.LogInfo("My Mod loaded!");
    }
}
```

The `[BepInPlugin]` attribute takes three arguments:

1. **GUID** — unique identifier (reverse domain notation recommended)
2. **Name** — human-readable mod name
3. **Version** — semantic version string

> **Important:** `BaseUnityPlugin` inherits from Unity's `MonoBehaviour`. This means your
> plugin class has access to the full Unity lifecycle — `Update()`, `OnDestroy()`, `OnGUI()`,
> and so on. If your mod needs per-frame logic (input handling, overlay rendering), you can
> implement those methods directly in your plugin class.

---

## New patch types

Harmony 2 adds two patch types that don't exist in Harmony 1.x. Prefix, Postfix, and
Transpiler work the same as before.

### Finalizer

Runs after the original method, **even if it threw an exception**. Can suppress or
replace exceptions. Return `null` to suppress, or return the exception to let it propagate.

**Real example — silent exception swallowing in `AgentManager.OnFixedUpdate()`:**

The game updates every agent each tick, but silently swallows all exceptions:

```csharp
// From AgentManager.OnFixedUpdate() — the actual game code
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
            // Completely silent — no logging, no context, errors vanish
        }
    }
}
```

**Harmony 1 workaround** — a Prefix that replaces the entire method just to add logging:

```csharp
// H1: must replace the whole method with Prefix returning false
[HarmonyPrefix]
public static bool Prefix(AgentManager __instance)
{
    foreach (AgentModel agent in __instance.agentList)
    {
        try
        {
            agent.OnFixedUpdate();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Agent update failed: {ex}");
        }
    }

    return false; // Skip original — blocks other mods' patches on this method
}
```

**Harmony 2 Finalizer** — intercepts the exception without replacing anything:

```csharp
[HarmonyPatch(typeof(AgentManager), "OnFixedUpdate")]
public class AgentManagerOnFixedUpdateFinalizer
{
    [HarmonyFinalizer]
    public static Exception Finalizer(Exception __exception)
    {
        if (__exception != null)
        {
            Logger.LogError($"AgentManager.OnFixedUpdate threw: {__exception}");
        }

        return null; // Suppress the exception so other agents still update
    }
}
```

The Finalizer is surgical: it logs the error and suppresses it, without replacing the
original method. Other mods' Prefix/Postfix patches on `OnFixedUpdate` still work
normally.

### Reverse Patch

Creates a callable copy of an original method, letting you call private methods directly
from mod code — no reflection, no copy-pasting game logic that drifts between patches.

**Real example — calling the private XP formula in `UseSkill.CalculateLevelExp()`:**

The game calculates work XP multipliers in a private method. It compares the agent's stat
level against the creature's risk level, then applies a base rate and a penalty for the
battle (P) stat:

```csharp
// From UseSkill.CalculateLevelExp() — the actual game code (names cleaned up)
private float CalculateLevelExp(RwbpType rwbpType)
{
    WorkerPrimaryStat addedStat = agent.primaryStat.GetAddedStat(agent.primaryStatExp);
    float multiplier = 1f;
    int statLevel = 0;

    switch (rwbpType)
    {
    case RwbpType.R: statLevel = AgentModel.CalculateStatLevel(addedStat.hp); break;
    case RwbpType.W: statLevel = AgentModel.CalculateStatLevel(addedStat.mental); break;
    case RwbpType.B: statLevel = AgentModel.CalculateStatLevel(addedStat.work); break;
    case RwbpType.P: statLevel = AgentModel.CalculateStatLevel(addedStat.battle); break;
    }

    // Higher stat level relative to creature risk = less XP
    switch (statLevel - targetCreature.GetRiskLevel())
    {
    case -3: multiplier = 1.4f; break;
    case -2: multiplier = 1.2f; break;
    case -1: case 0: multiplier = 1f; break;
    case 1: multiplier = 0.8f; break;
    case 2: multiplier = 0.6f; break;
    case 3: multiplier = 0.4f; break;
    case 4: multiplier = 0.2f; break;
    }

    float[] baseExpRates = { 0.6f, 0.55f, 0.5f, 0.45f, 0.4f };
    if (statLevel <= 0 || statLevel > baseExpRates.Length)
        return multiplier * baseExpRates[0];

    multiplier *= baseExpRates[statLevel - 1];
    if (rwbpType == RwbpType.P)
        multiplier /= 3f;

    return multiplier;
}
```

A "training optimizer" mod wants to preview XP gains before assigning work. It needs the
exact game formula — but the method is `private`.

**Harmony 1 workaround** — reflection on every call:

```csharp
// H1: must use reflection to call the private method
var method = AccessTools.Method(typeof(UseSkill), "CalculateLevelExp");
float result = (float)method.Invoke(useSkillInstance, new object[] { RwbpType.R });
// Slow per call, no compile-time type safety, breaks silently if the method is renamed
```

**Harmony 2 Reverse Patch** — a type-safe, direct call:

```csharp
[HarmonyPatch(typeof(UseSkill))]
public class UseSkillReversePatch
{
    [HarmonyReversePatch]
    [HarmonyPatch("CalculateLevelExp")]
    public static float CalculateLevelExp(UseSkill instance, RwbpType rwbpType)
    {
        // Stub — replaced at runtime with the original method's code
        throw new NotImplementedException();
    }
}

// Call it like a normal static method — type-safe, no reflection
float xpMultiplier = UseSkillReversePatch.CalculateLevelExp(useSkill, RwbpType.R);
```

The first parameter is the instance (since `CalculateLevelExp` is an instance method);
remaining parameters match the original signature. The Reverse Patch gives compile-time
type safety, no per-call reflection overhead, and stays in sync with the game code even
if other mods patch it. Use `HarmonyReversePatchType.Original` if you need the unpatched
version specifically.

---

## Integration with LMM

### Deploying to BaseMods

Place your compiled mod DLL in the game's `BaseMods/` folder, just like any LMM mod.
RetargetHarmony handles the assembly reference rewriting at load time, so the game's
mod loader picks up your DLL normally.

### Coexistence with Harmony 1 mods

RetargetHarmony rewrites assembly references so that both Harmony 1.x and Harmony 2
mods resolve to the correct runtime library. You do not need to do anything special —
your Harmony 2 mod will work alongside existing Harmony 1 mods.

### What to avoid

- **Do not bundle `0Harmony.dll` or `BepInEx.dll`** in your mod — the game already has
  them. Set `<Private>false</Private>` on those references.
- **Do not patch generic methods without special handling** — generic method patches
  require you to specify the concrete type parameters. Omitting them produces cryptic
  `InvalidOperationException` or `AmbiguousMatchException` errors at patch time.
- **Do not patch coroutines (IEnumerator methods) by name alone** — Unity coroutines
  compile to hidden state machine classes. You need to patch the generated
  `MoveNext()` method on the compiler-generated class, not the method that returns
  `IEnumerator`. Use a tool like dnSpy or ILSpy to find the generated class name.
- **It is highly recommended to not target a framework newer than .NET Framework 3.5** — the game's Mono runtime does not
  officially support it, and some features of .NET Framework 4 will cause your mod to crash.

---

## CodeMatcher — pattern matching for transpilers

Harmony 2's `CodeMatcher` is a fluent API for finding and modifying IL patterns. Instead
of iterating every instruction manually, you describe the pattern and CodeMatcher finds
it — with built-in error reporting when patterns aren't found.

**Real example — replacing the 95% work success cap:**

In `UseSkill.ProcessWorkTick()`, after
all work probability bonuses are calculated, the game clamps success rate to 95%:

```csharp
// From UseSkill.ProcessWorkTick() — the actual game code
private void ProcessWorkTick(out bool isSuccess)
{
    // ... calculate workSuccessProb from agent stats, creature bonuses, equipment ...
    workSuccessProb = targetCreature.script.TranformWorkProb(workSuccessProb);

    if (workSuccessProb > 0.95f)       // ← the cap
    {
        workSuccessProb = 0.95f;       // ← clamp to 95%
    }

    // ... apply penalties, random roll, success/fail logic ...
}
```

Using CodeMatcher to replace both occurrences of the constant:

```csharp
[HarmonyPatch(typeof(UseSkill), "ProcessWorkTick")]
public class ProcessWorkTickTranspiler
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_R4, 0.95f))
            .ThrowIfInvalid("Could not find 0.95f success cap")
            .SetOperandAndAdvance(1.0f)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_R4, 0.95f))
            .ThrowIfInvalid("Could not find second 0.95f constant")
            .SetOperandAndAdvance(1.0f)
            .InstructionEnumeration();
    }
}
```

Key advantages over manual IL iteration:

- **`ThrowIfInvalid`** fails loudly if the pattern isn't found (e.g., after a game update
  changes the IL), instead of silently doing nothing
- **`MatchForward`/`MatchBack`** let you search by context — match multiple instructions
  in sequence to avoid hitting the wrong constant
- **Chainable** — reads top to bottom as a series of "find, then modify" steps

To make this configurable instead of hardcoded to `1.0f`, see the
[Configuration](#configuration) section for a complete Config + Transpiler example.

### Transpiler debugging tips

- Use `Logger.LogDebug` to print each instruction before and after modification.
- Use dnSpy or ILSpy to read the original IL — do not guess at the instruction sequence.
- If the game updates and your transpiler silently breaks, add a validation pass that
  checks whether the expected instruction pattern exists before modifying anything.

---

## BepInEx features

### Logging

The game's own error handling uses Unity's `Debug.Log` / `Debug.LogError`, which gives
you a stack trace but no structured context. Here's a real example from
`CreatureManager.OnFixedUpdate()`:

```csharp
// Game code — CreatureManager.OnFixedUpdate()
public void OnFixedUpdate()
{
    try
    {
        foreach (CreatureModel creature in creatureList)
        {
            try
            {
                creature.OnFixedUpdate();
            }
            catch (Exception message)
            {
                Debug.LogError(message);  // Which creature? What mod caused this?
            }
        }
    }
    catch (InvalidOperationException ex)
    {
        Debug.LogError("list modified" + ex);  // No context about what modified the list
    }
}
```

The inner catch logs the exception but not *which* creature threw it. The outer catch
says "list modified" but not what added or removed the entry. If you have several mods
patching creature behavior, `Debug.LogError` output all blends together in Unity's
`output_log.txt` with no source tagging and no log levels.

**BepInEx logging** solves all of these problems:

```csharp
// Available log levels — each level can be independently filtered
Logger.LogDebug("Verbose detail for development");     // LogLevel.Debug
Logger.LogInfo("Normal operational messages");          // LogLevel.Info
Logger.LogMessage("Important messages always shown");   // LogLevel.Message
Logger.LogWarning("Potential issues that aren't fatal");// LogLevel.Warning
Logger.LogError("Errors that need attention");          // LogLevel.Error
Logger.LogFatal("Unrecoverable errors");                // LogLevel.Fatal
```

Every line BepInEx logs is automatically tagged with your plugin's name:

```
[Info   :   XP Overhaul] Patched UseSkill.ProcessWorkTick successfully
[Warning: Difficulty Mod] Config value MaxSuccessRate clamped to 0.99
[Error  :   XP Overhaul] CreatureModel.OnFixedUpdate threw for Snow Queen (ID: 42): ...
```

Compare the same error with `Debug.LogError`:

```
NullReferenceException: Object reference not set to an instance of an object
  at CreatureModel.OnFixedUpdate () ...
```

No mod name, no creature identity, no severity level — just a raw exception dumped into
the same log file as every other Unity message.

Log output goes to `BepInEx/LogOutput.log` and (if enabled) the BepInEx console. You can
filter log levels in `BepInEx/config/BepInEx.cfg` under `[Logging.Console]` and
`[Logging.Disk]`.

### Configuration

BepInEx provides a built-in configuration system:

```csharp
private ConfigEntry<int> _damageMultiplier;

private void Awake()
{
    _damageMultiplier = Config.Bind(
        "General",           // Section
        "DamageMultiplier",  // Key
        1,                   // Default value
        "Multiplier applied to all damage" // Description
    );

    Logger.LogInfo($"Damage multiplier: {_damageMultiplier.Value}");
}
```

Config files are auto-generated at `BepInEx/config/com.yourname.mymod.cfg`. These files
are **human-editable** — users can open them in any text editor to change settings without
rebuilding the mod.

To react to config changes at runtime without requiring a game restart:

```csharp
_damageMultiplier.SettingChanged += (sender, args) =>
{
    Logger.LogInfo($"Damage multiplier changed to {_damageMultiplier.Value}");
};
```

**Real example — making the work success cap configurable (Config + CodeMatcher):**

Pairing a `ConfigEntry` with a CodeMatcher transpiler lets players set the max success
rate by editing a config file — no rebuild needed:

```csharp
[BepInPlugin("com.yourname.difficultymod", "Difficulty Mod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<float> MaxSuccessRate;

    private void Awake()
    {
        MaxSuccessRate = Config.Bind(
            "Difficulty",
            "MaxSuccessRate",
            0.95f,
            "Maximum work success probability (0.0 to 1.0)");

        var harmony = new Harmony("com.yourname.difficultymod");
        harmony.PatchAll();
    }

    // Called by the Transpiler at runtime instead of the hardcoded 0.95f
    public static float GetMaxSuccessRate() => MaxSuccessRate.Value;
}
```

The Transpiler replaces the `ldc.r4 0.95f` instruction with a method call, so the config
value is read fresh each time the game checks the cap:

```csharp
[HarmonyPatch(typeof(UseSkill), "ProcessWorkTick")]
public class ProcessWorkTickTranspiler
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var getMaxRate = AccessTools.Method(
            typeof(Plugin), nameof(Plugin.GetMaxSuccessRate));

        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_R4, 0.95f))
            .SetInstructionAndAdvance(
                new CodeInstruction(OpCodes.Call, getMaxRate))
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_R4, 0.95f))
            .SetInstructionAndAdvance(
                new CodeInstruction(OpCodes.Call, getMaxRate))
            .InstructionEnumeration();
    }
}
```

Players edit `BepInEx/config/com.yourname.difficultymod.cfg`:

```ini
[Difficulty]

## Maximum work success probability (0.0 to 1.0)
# Setting type: Single
# Default value: 0.95
MaxSuccessRate = 1.0
```

This Config + Transpiler pipeline is the most common pattern for mods that expose
game constants as player settings.

### Dependency declarations

BepInEx manages mod load order through dependency declarations. Without them, mods
load in an undefined order — if your mod patches a method that another mod also
patches, results vary depending on which mod loads first.

**Hard dependencies** — BepInEx will not load your mod if the dependency is missing,
and guarantees it loads first:

```csharp
[BepInPlugin("com.yourname.mymod", "My Mod", "1.0.0")]
[BepInDependency("com.otherauthor.requiredmod")]
public class Plugin : BaseUnityPlugin { }
```

**Soft dependencies** are more common in LC modding. Consider a realistic scenario:
you've written an XP Overhaul mod that patches `UseSkill.FinishWorkSuccessfully()` to
change experience formulas (building on the Transpiler example above). Another modder
releases a Custom Creatures mod that adds creatures with non-standard risk levels.
Your XP formula uses risk levels, so you want to detect and adapt — but your mod
should still work without theirs.

```csharp
[BepInPlugin("com.example.xpoverhaul", "XP Overhaul", "1.0.0")]
[BepInDependency("com.example.customcreatures",
    BepInDependency.DependencyFlags.SoftDependency)]
public class XpOverhaulPlugin : BaseUnityPlugin
{
    internal static bool HasCustomCreatures { get; private set; }

    private void Awake()
    {
        // Check at startup whether the optional mod is loaded
        HasCustomCreatures = BepInEx.Bootstrap.Chainloader.PluginInfos
            .ContainsKey("com.example.customcreatures");

        if (HasCustomCreatures)
        {
            Logger.LogInfo("Custom Creatures detected — adjusting XP formula " +
                "for non-standard risk levels");
        }

        var harmony = new Harmony("com.example.xpoverhaul");
        harmony.PatchAll();
    }
}
```

With a soft dependency, BepInEx ensures load order (Custom Creatures loads first *if
present*) but won't block your mod if it's missing. The `Chainloader.PluginInfos`
check at startup tells you which code path to take.

Your patches can then branch on the flag:

```csharp
[HarmonyPatch(typeof(UseSkill), nameof(UseSkill.FinishWorkSuccessfully))]
public static class UseSkillPatchFinishWorkSuccessfully
{
    public static void Postfix(UseSkill __instance)
    {
        if (XpOverhaulPlugin.HasCustomCreatures)
        {
            // Custom creatures use extended risk levels (6-10) that the base
            // XP formula doesn't handle — apply adjusted multipliers
        }
        else
        {
            // Standard XP override for vanilla creatures (risk levels 1-5)
        }
    }
}
```

> **Tip:** Always check for the optional mod once at startup and cache the result.
> Calling `Chainloader.PluginInfos.ContainsKey()` on every patch invocation works but
> wastes cycles on a value that never changes after load.

---

## Preloader patchers

BepInEx supports assembly-level patching via Preloader patchers. These run **before**
the game's assemblies are loaded into the runtime, enabling modifications that Harmony
cannot do — such as adding fields to existing types, changing constants, modifying
assembly attributes, or injecting entirely new types.

> **Caution:** Preloader patchers interact with assemblies at a lower level than Harmony.
> Their compatibility with LMM's RetargetHarmony layer has not been exhaustively tested.
> If you use preloader patchers, test thoroughly and be prepared for edge cases.

**Real example — adding per-creature mod data to `CreatureModel`:**

Modders often need to store custom data on each creature — a custom difficulty level,
tracking flags, or mod-specific state. Without a Preloader, the only option is an
external dictionary keyed by instance ID:

```csharp
// Without Preloader: external dictionary approach
// Fragile — not garbage-collected with the creature, must manually clean up
private static readonly Dictionary<long, int> CustomDifficulty = new Dictionary<long, int>();

[HarmonyPostfix]
[HarmonyPatch(typeof(CreatureModel), "Init")]
public static void Postfix(CreatureModel __instance)
{
    CustomDifficulty[__instance.metadataId] = CalculateDifficulty(__instance);
}

// Must also patch creature removal to avoid memory leaks:
[HarmonyPostfix]
[HarmonyPatch(typeof(CreatureManager), "RemoveCreature")]
public static void RemovePostfix(CreatureModel creature)
{
    CustomDifficulty.Remove(creature.metadataId);
}

// Every access requires a dictionary lookup + existence check
public static int GetDifficulty(CreatureModel creature)
{
    return CustomDifficulty.TryGetValue(creature.metadataId, out var diff) ? diff : 0;
}
```

With a Preloader patcher, you can add the field directly to `CreatureModel` using
Mono.Cecil. The field lives on the object itself — no dictionary, no manual cleanup,
no risk of stale entries:

```csharp
using System.Collections.Generic;
using BepInEx.Logging;
using Mono.Cecil;

public static class CreatureFieldPatcher
{
    public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

    public static void Patch(AssemblyDefinition assembly)
    {
        var logger = Logger.CreateLogSource("CreatureFieldPatcher");

        // Find CreatureModel in Assembly-CSharp
        TypeDefinition creatureModel = null;
        foreach (var type in assembly.MainModule.Types)
        {
            if (type.Name == "CreatureModel")
            {
                creatureModel = type;
                break;
            }
        }

        if (creatureModel == null)
        {
            logger.LogError("CreatureModel type not found");
            return;
        }

        // Add a public int field for custom difficulty
        var intRef = assembly.MainModule.ImportReference(typeof(int));
        var field = new FieldDefinition(
            "customDifficultyLevel",
            FieldAttributes.Public,
            intRef);
        creatureModel.Fields.Add(field);

        logger.LogInfo("Added customDifficultyLevel field to CreatureModel");
    }
}
```

Now your Harmony patches can read and write the field directly:

```csharp
// With Preloader: the field exists on CreatureModel at runtime
[HarmonyPostfix]
[HarmonyPatch(typeof(CreatureModel), "Init")]
public static void Postfix(CreatureModel __instance)
{
    // Direct field access — no dictionary, no cleanup needed
    __instance.customDifficultyLevel = CalculateDifficulty(__instance);
}
```

Preloader patchers are separate DLLs placed in `BepInEx/patchers/` (not `plugins/`).
They use **Mono.Cecil** (bundled with BepInEx) to manipulate assemblies as data, before
the CLR loads them. This is fundamentally different from Harmony's runtime method hooking.

The required contract is:
- `public static IEnumerable<string> TargetDLLs` — which assemblies to patch
- `public static void Patch(AssemblyDefinition assembly)` — the patch logic
- Optionally, `public static void Finish()` — runs after all `Patch()` calls complete

> **Reference implementation:** `RetargetHarmony.cs` in this repository is a real working
> preloader patcher that retargets Harmony assembly references using Mono.Cecil. Study it
> for patterns around error handling, logging, and the `TargetDLLs`/`Patch`/`Finish`
> lifecycle.

---

## Debugging

### Log files

- **`BepInEx/LogOutput.log`** — main log file, recreated each game launch
- **`output_log.txt`** — Unity's log file

### Common errors

| Error | Cause | Fix |
|-------|-------|-----|
| `TypeLoadException` | Missing assembly reference | Ensure all referenced DLLs are available |
| `MissingMethodException` | Game updated, method signature changed | Update your `[HarmonyPatch]` target |
| `FileNotFoundException: 0Harmony` | Harmony DLL not found | Reinstall Harmony 2 for LMM |
| `InvalidOperationException` on generic type | Patching a generic method without type params | Specify concrete type arguments |
| `AmbiguousMatchException` | Multiple overloads match the patch target | Use `TargetMethod()` or specify parameter types in the attribute |
| Patch not applied (no error) | Wrong method name or signature | Double-check `typeof` and `nameof` targets |
| Coroutine patch has no effect | Patching the IEnumerator method, not `MoveNext()` | Use dnSpy to find the generated state machine class |
| Transpiler silently breaks after update | IL instruction sequence changed | Add a validation pass to check expected patterns before modifying |

## Version reference

| Component | Version |
|-----------|---------|
| BepInEx | 5.4.23.5 |
| HarmonyX (Harmony 2) | Bundled with BepInEx |
| .NET Framework | 3.5 (Unity 2017.4 Mono) |
| Unity | 2017.4 |