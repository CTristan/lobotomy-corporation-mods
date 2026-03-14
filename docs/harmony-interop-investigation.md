# Investigation: BepInEx HarmonyInteropFix Permanently Modifying BaseMods DLLs

## Problem Statement

BaseMods DLLs show `0Harmony109` assembly references on disk after the game runs, even though RetargetHarmony only patches in-memory `AssemblyDefinition` objects and never calls `assembly.Write()`. After restoring original DLLs from their source and running the game once, the modification reappears. Binary inspection confirms the DLLs are completely re-serialized by Mono.Cecil (the entire PE structure differs, not just the reference name).

## Investigation Methodology

1. Searched all decompiled game source (`external/decompiled/Assembly-CSharp/`) for assembly write operations
2. Reviewed RetargetHarmony source for disk write paths
3. Decompiled bundled BepInEx 5.4.23.5 DLLs (`BepInEx.Preloader.dll`, `BepInEx.dll`, `HarmonyXInterop.dll`, `BepInEx.Harmony.dll`) using `ilspycmd`
4. Traced the complete assembly loading chain from Doorstop → BepInEx Preloader → Game startup → `Add_On.cs`
5. Compared binary contents of original vs modified DLLs using `strings` and `xxd`

## Root Cause: BepInEx's Built-in `HarmonyInteropFix`

**It is NOT RetargetHarmony doing this — it is BepInEx itself.**

### The Chain of Events

```
1. Doorstop (winhttp.dll) intercepts Mono JIT init
2. Doorstop loads BepInEx.Preloader.dll
3. BepInEx.Preloader.Preloader.Run() executes:
   → Line 374: HarmonyInteropFix.Apply()
     → HarmonyInterop.Initialize(Paths.CachePath)
       → Discovers 0Harmony109.dll, 0Harmony12.dll in BepInEx/core/
       → Registers them as interop assembly names
     → Harmony.CreateAndPatchAll(typeof(HarmonyInteropFix))
       → Patches Assembly.LoadFile(string) with OnAssemblyLoad prefix
       → Patches Assembly.LoadFrom(string) with OnAssemblyLoad prefix
4. BepInEx preloader continues: loads patchers, patches Managed/ assemblies
5. BepInEx chainloader starts, loads plugins
6. Game starts → Add_On.init() runs
   → Line 109: Assembly.LoadFile(fileInfo.FullName) for each BaseMods DLL
     → HarmonyInteropFix.OnAssemblyLoad prefix fires BEFORE actual load
     → Calls HarmonyInterop.TryShim(path, gameRootPath, ...)
       → TryShimInternal():
         a. Reads DLL bytes from disk
         b. Reads with Mono.Cecil: AssemblyDefinition.ReadAssembly(memoryStream)
         c. Finds 0Harmony reference not in interop names
         d. Renames: harmonyRef.Name = "0Harmony109"
         e. Writes to MemoryStream: assemblyDefinition.Write(memoryStream)
         f. BACKS UP ORIGINAL: File.WriteAllBytes(backupPath, originalBytes)
         g. OVERWRITES ON DISK: File.WriteAllBytes(path, memoryStream.ToArray())
         h. Updates timestamp cache to prevent re-shimming
```

### Decompiled Evidence

**Source: `BepInEx.Preloader.dll` → `HarmonyInteropFix` class**

```csharp
internal static class HarmonyInteropFix
{
    public static void Apply()
    {
        HarmonyInterop.Initialize(Paths.CachePath);
        Harmony.CreateAndPatchAll(typeof(HarmonyInteropFix), "org.bepinex.fixes.harmonyinterop");
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(Assembly), "LoadFile", new Type[] { typeof(string) })]
    private static Assembly LoadFile(string path)
    {
        return null;  // Reverse patch — calls original unpatched LoadFile
    }

    [HarmonyPatch(typeof(Assembly), "LoadFile", new Type[] { typeof(string) })]
    [HarmonyPatch(typeof(Assembly), "LoadFrom", new Type[] { typeof(string) })]
    [HarmonyPrefix]
    private static bool OnAssemblyLoad(ref Assembly __result, string __0)
    {
        HarmonyInterop.TryShim(__0, Paths.GameRootPath, Logger.LogWarning, TypeLoader.ReaderParameters);
        __result = LoadFile(__0);
        return true;
    }
}
```

**Source: `HarmonyXInterop.dll` → `HarmonyInterop.TryShimInternal()` (key excerpt)**

```csharp
private static byte[] TryShimInternal(string path, string gameRootDirectory, ...)
{
    // ... reads DLL, finds Harmony references ...

    AssemblyNameReference harmonyRef = assemblyDefinition.MainModule.AssemblyReferences
        .FirstOrDefault(a => a.Name.StartsWith("0Harmony") && !InteropAssemblyNames.Contains(a.Name));

    if (harmonyRef != null)
    {
        // Renames reference to interop name
        harmonyRef.Name = keyValuePair.Value;  // e.g., "0Harmony109"

        using MemoryStream memoryStream = new MemoryStream();
        assemblyDefinition.Write(memoryStream);

        try
        {
            // Creates backup of original
            string backupDir = Path.Combine(gameRootDirectory, "BepInEx_Shim_Backup");
            Directory.CreateDirectory(backupDir);
            File.WriteAllBytes(Path.Combine(backupDir, Path.GetFileName(path)), originalBytes);

            // *** OVERWRITES ORIGINAL DLL ON DISK ***
            File.WriteAllBytes(path, memoryStream.ToArray());
        }
        catch (IOException) { }
    }

    // Updates timestamp cache to skip on next launch
    shimCache[path] = File.GetLastWriteTimeUtc(path).Ticks;
}
```

### Why Only BaseMods DLLs Are Affected

| Assembly | Load Mechanism | Goes Through Assembly.LoadFile()? | Shimmed on Disk? |
|----------|---------------|--------------------------------------|----------|
| Assembly-CSharp.dll | BepInEx preloader → `Assembly.Load(byte[])` | No | No |
| LobotomyBaseModLib.dll | BepInEx preloader → `Assembly.Load(byte[])` | No | No |
| BaseMods/*.dll | Game's `Add_On.cs` → `Assembly.LoadFile()` | **Yes** | **Yes** |

The BepInEx preloader uses a completely different loading path for managed assemblies:

```csharp
// BepInEx AssemblyPatcher.Load() — writes to MemoryStream, NOT disk
public static void Load(AssemblyDefinition assembly, string filename)
{
    using MemoryStream memoryStream = new MemoryStream();
    assembly.Write(memoryStream);
    Assembly.Load(memoryStream.ToArray());  // No disk write
}
```

### Binary Comparison Evidence

**Original DLL (`dll_test/original/abcdcode_ExtendConsole_MOD.dll`)**:
```
$ strings original.dll | grep -i harmony
HarmonyMethod
HarmonyInstance
Harmony_Patch
0Harmony          ← Original reference
```

**Modified DLL (`dll_test/modified/abcdcode_ExtendConsole_MOD.dll`)**:
```
$ strings modified.dll | grep -i harmony
0Harmony109        ← Rewritten by TryShim
HarmonyMethod
HarmonyInstance
Harmony_Patch
Harmony
```

The binary diff (`xxd` comparison) shows extensive changes across the PE structure — characteristic of Mono.Cecil re-serialization where the entire assembly is rewritten, not just the reference name bytes.

### What RetargetHarmony Actually Does for BaseMods

**Nothing through the preloader path.** BaseMods DLLs are not in `Managed/`, so BepInEx's `AssemblyPatcher.PatchAndLoad()` never finds them:

```csharp
// BepInEx AssemblyPatcher.PatchAndLoad — only reads from Managed/
foreach (string uniqueFilesInDirectory in Utility.GetUniqueFilesInDirectories(directories, "*.dll"))
{
    // directories = Paths.DllSearchPaths = [Managed/]
    // BaseMods DLLs are NOT here → never loaded → never patched
}
```

RetargetHarmony's `TargetDLLs` yields BaseMods filenames, but `PatchAndLoad` looks up these names in its dictionary (keyed by Managed/ files) — they're never found, so `Patch()` is never called for BaseMods DLLs.

### Shim Caching Mechanism

HarmonyInteropFix maintains a cache at `BepInEx/cache/harmony_interop_cache.dat`:
- Stores `{filePath → lastWriteTimeTicks}` pairs
- On subsequent launches, skips shimming if file timestamp matches
- Restoring original DLLs changes the timestamp → triggers re-shimming
- Cache is a binary format read/written via `BinaryReader`/`BinaryWriter`

Backups are stored in `{gameRoot}/BepInEx_Shim_Backup/` preserving the relative directory structure from the game root.

## What Was Ruled Out

### Decompiled Game Code (`external/decompiled/Assembly-CSharp/`)
- **Add_On.cs**: Uses `Assembly.LoadFile()` — read-only operation
- **SaveUtil.cs**: Only serializes game save data (Dictionary<string, object>)
- **ModSaveUtil.cs**: Mod-specific save data only
- **ModDebug.cs**: Writes text logs, not DLLs
- **GlobalGameManager.cs**: Game state saves only
- No code anywhere in the decompiled source writes assemblies or DLLs to disk

### RetargetHarmony
- `Patch(AssemblyDefinition asm)`: Modifies `AssemblyNameReference.Name` in-memory only — no `Write()` call
- `OnAssemblyResolve`: Uses `AssemblyDefinition.ReadAssembly()` — read-only
- `WriteAuditLog`: Writes text to `patched_mods.log` — not DLL modification
- Config file generation: Writes text config, not DLLs

### BepInEx Preloader (`AssemblyPatcher.PatchAndLoad`)
- Writes patched assemblies to `MemoryStream` → `Assembly.Load(bytes)` — never back to disk
- `DumpAssemblies` feature writes to `BepInEx/DumpedAssemblies/` — separate directory, not original location
- Not enabled in bundled configuration

### BepInEx TypeLoader (`TypeLoader.FindPluginTypes`)
- Read-only: reads assemblies with Mono.Cecil, scans types, disposes
- `SaveAssemblyCache`: Writes a `.dat` metadata cache file, not assembly DLLs

### SetupExternal/AssemblyRetargeter.cs
- DOES write DLLs to disk using `assembly.Write()` — but only targets mscorlib references
- Dev-only tool, not distributed to users

### scripts/rename-assembly.csx
- DOES write DLLs to disk — but only renames assembly names for Common versioning
- Build-time script, not runtime

---

## BepInEx Benefits for Future Mod Development

### What Basemod/LMM Provides
- Simple mod loading from `BaseMods/` directory via `Add_On.cs`
- XML-based load ordering (`BaseModList_v2.xml`)
- Two config types: Toggle (bool) and Slider (float) via `ModOptionManager`
- Single unstructured log file via `ModDebug`
- `Harmony_Patch` class instantiation for patching
- `ModInitializer` base class for custom initialization

### What BepInEx Adds

**Dependency Management**
- `[BepInDependency]` attribute with GUID and version constraints
- Topological sort of plugin load order
- Circular dependency detection
- Version-aware loading with `MinimumVersion` checks
- Incompatibility declarations via `[BepInIncompatibility]`

**Configuration System**
- Typed `ConfigEntry<T>` bindings with defaults and descriptions
- Per-plugin isolated config files (`BepInEx/config/{GUID}.cfg`)
- Auto-generated config files with comments
- Supports primitives, enums, custom types
- Change callbacks for runtime config updates

**Logging Infrastructure**
- Per-plugin `ManualLogSource` with plugin name prefix
- Structured log levels: Trace, Debug, Info, Warning, Error, Fatal
- Thread-safe I/O serialization
- Configurable per-level filtering
- Console + file output simultaneously

**Plugin Lifecycle**
- Full MonoBehaviour lifecycle: `Awake → OnEnable → Update → OnDisable → OnDestroy`
- Per-frame work capability via `Update()`
- Clean resource cleanup via `OnDestroy()`
- Runtime enable/disable support

**Error Isolation**
- Chainloader wraps each plugin in try/catch
- One plugin failure doesn't crash others
- Per-plugin error logging with identification
- Degraded startup mode

**Pre-Game Patching (Preloader)**
- IL modification via Mono.Cecil before runtime loads assemblies
- Assembly metadata rewriting
- Custom assembly resolution handlers
- Cannot be achieved by Basemod (loads after game initialization)

**Inter-Mod Dependencies**
- Mods can reference each other's public types
- Shared code libraries via declared dependencies
- Assembly resolution across plugin directories
