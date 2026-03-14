# Plan: Prevent BepInEx from Permanently Modifying BaseMods DLLs

## Context

BaseMods DLLs are being permanently rewritten on disk when the game runs with BepInEx installed. The root cause is **BepInEx's built-in `HarmonyInteropFix`** (in `HarmonyXInterop.dll`), NOT RetargetHarmony.

**The chain:** BepInEx patches `Assembly.LoadFile()` with a Harmony prefix. When the game's `Add_On.cs:109` calls `Assembly.LoadFile()` for each BaseMods DLL, `HarmonyInterop.TryShim()` fires — reads the DLL, renames `0Harmony` → `0Harmony109`, and **writes the modified DLL back to disk** via `File.WriteAllBytes(path, ...)`. Assembly-CSharp.dll and LobotomyBaseModLib.dll are unaffected because BepInEx loads them via `Assembly.Load(byte[])` which bypasses the hook.

**Key decompiled evidence** (from bundled `BepInEx.Preloader.dll` v5.4.23.5):
```csharp
// HarmonyInteropFix — patches Assembly.LoadFile globally
[HarmonyPatch(typeof(Assembly), "LoadFile", new Type[] { typeof(string) })]
[HarmonyPatch(typeof(Assembly), "LoadFrom", new Type[] { typeof(string) })]
[HarmonyPrefix]
private static bool OnAssemblyLoad(ref Assembly __result, string __0)
{
    HarmonyInterop.TryShim(__0, Paths.GameRootPath, ...);  // WRITES TO DISK
    __result = LoadFile(__0);
    return true;
}
```

## Goals

1. BaseMods DLLs on disk are never modified by RetargetHarmony or BepInEx
2. Harmony 1.x BaseMods still work correctly alongside Harmony 2.x
3. BepInEx plugin ecosystem remains available for future mod development
4. Investigation findings are documented for reference

## Approach: Neutralize HarmonyInteropFix + Non-Destructive In-Memory Hook

In RetargetHarmony's `Finish()`, unpatch BepInEx's `HarmonyInteropFix` from `Assembly.LoadFile/LoadFrom`, then apply our own prefix that does the same retargeting **in memory only** — never writing to disk.

**Why this works:**
- All consumers of loaded assemblies (`Add_On.AssemList`) only call `assem.GetTypes()` — none access `.Location` or `.CodeBase`
- `Assembly.Load(byte[])` produces a fully functional assembly for type discovery
- RetargetHarmony already has the Mono.Cecil retargeting logic in `Patch()`

## Tasks

### Phase 1: Documentation

- [ ] Save investigation findings to `docs/harmony-interop-investigation.md`
  - Root cause analysis (HarmonyInteropFix chain of events)
  - Decompiled evidence from BepInEx.Preloader.dll and HarmonyXInterop.dll
  - Why only BaseMods DLLs are affected
  - BepInEx benefits analysis (dependency management, config, logging, lifecycle, pre-game patching, error isolation, inter-mod dependencies)

### Phase 2: Implement Non-Destructive Assembly Loading

- [ ] Add method to `RetargetHarmony.cs` to unpatch HarmonyInteropFix
  - Use `Harmony.Unpatch` targeting `Assembly.LoadFile` and `Assembly.LoadFrom` with patch owner `"org.bepinex.fixes.harmonyinterop"`
  - Must run in `Finish()` AFTER `HarmonyInteropFix.Apply()` has already been called

- [ ] Add our own `Assembly.LoadFile` prefix that does in-memory-only retargeting
  - Read file bytes with `File.ReadAllBytes(path)`
  - Use Mono.Cecil on byte stream to check for Harmony references needing retargeting
  - If retargeting needed: patch in memory, return `Assembly.Load(patchedBytes)`, skip original
  - If no retargeting needed: let original `LoadFile` proceed
  - Cache results to avoid re-patching the same assembly
  - NEVER call `File.WriteAllBytes` or any disk write

- [ ] Reuse existing `Patch(AssemblyDefinition)` logic for the in-memory retargeting
  - File: `RetargetHarmony/RetargetHarmony.cs:331-406`

### Phase 3: Remove Redundant PatchBaseMods from TargetDLLs

- [ ] Remove BaseMods DLL yielding from `TargetDLLs` property
  - The preloader path never actually patches BaseMods (they're not in `Managed/`)
  - The new `LoadFile` prefix handles BaseMods retargeting instead
  - Keep `PatchBaseModsEnabled` config to control whether the `LoadFile` prefix is active

### Phase 4: Restore Already-Shimmed DLLs (Optional)

- [ ] Consider adding restore logic for DLLs already modified by HarmonyInteropFix
  - Check `BepInEx_Shim_Backup/` for original copies
  - Restore on first run after update
  - Or document manual restore process

### Phase 5: Update Tests

- [ ] Add tests for the new `Assembly.LoadFile` prefix behavior
  - Test: DLL with `0Harmony` reference gets retargeted in memory
  - Test: DLL without Harmony references passes through unchanged
  - Test: DLL already referencing `0Harmony109` passes through unchanged
  - Test: No disk writes occur during retargeting

- [ ] Update existing tests if `TargetDLLs` behavior changes

### Phase 6: Update Installer/Uninstaller

- [ ] Update `UninstallerService.cs` to restore from `BepInEx_Shim_Backup/` if present
  - File: `RetargetHarmony.Installer/Services/UninstallerService.cs`

- [ ] Update `Harmony2-uninstall.bat` similarly
  - File: `RetargetHarmony/Harmony2-uninstall.bat`

## Key Files

| File | Role |
|------|------|
| `RetargetHarmony/RetargetHarmony.cs` | Main patcher — add unpatch + new prefix in `Finish()` |
| `RetargetHarmony/DebugLogger.cs` | Logging abstraction (no changes needed) |
| `RetargetHarmony.Test/Tests/RetargetHarmonyTests.cs` | Add new tests |
| `RetargetHarmony.Installer/Services/UninstallerService.cs` | Restore logic |
| `RetargetHarmony/Harmony2-uninstall.bat` | Restore logic |
| `external/decompiled/Assembly-CSharp/Add_On.cs:109,139` | Game's `Assembly.LoadFile()` calls (read-only reference) |

## Verification

1. Build RetargetHarmony with changes
2. Install on clean game with fresh BaseMods DLLs containing `0Harmony` references
3. Run the game
4. Verify BaseMods DLLs on disk still show `0Harmony` (NOT `0Harmony109`)
5. Verify game runs correctly — Harmony 1.x mods work alongside Harmony 2.x
6. Run `dotnet test` — all existing + new tests pass
7. Check that no `BepInEx_Shim_Backup/` directory is created (shimming disabled)

## Future Consideration: Removing BepInEx Entirely

If BepInEx plugin support proves unnecessary, a standalone Doorstop approach would eliminate the problem at its root. This would involve:

- Using Doorstop 4.5.0 directly (already bundled) to inject a custom preloader
- Replacing `TypeLoader`, `Paths`, and `ManualLogSource` with custom implementations
- The game's `Add_On.cs` already loads BaseMods natively — no TypeLoader needed
- Our mods use `Harmony_Patch` class pattern (Basemod convention), not `[BepInPlugin]`

**Why keep BepInEx for now:** BepInEx provides dependency management, typed configuration, structured logging, full MonoBehaviour lifecycle, error isolation, pre-game IL patching, and inter-mod dependency support — none of which the Basemod system offers. These are valuable for future mod ecosystem growth.

## Risks & Considerations

- **HarmonyInteropFix version changes**: If BepInEx updates change the Harmony patch owner ID (`"org.bepinex.fixes.harmonyinterop"`), the unpatch call would silently fail. Pin BepInEx version or detect dynamically.
- **Other BepInEx plugins using Assembly.LoadFile**: Our prefix will intercept ALL `Assembly.LoadFile` calls. Need to ensure we only retarget BaseMods paths, not BepInEx plugin loads.
- **Assembly.Load(bytes) vs Assembly.LoadFile(path)**: The loaded assembly has no `Location` property. Verified safe — all game consumers only call `GetTypes()` on loaded assemblies.
- **Cache invalidation**: If a user updates a mod DLL, our cache must detect the change (use file hash or last-write timestamp, similar to HarmonyInteropFix's approach).
