# RetargetHarmony

A BepInEx 5 preloader patcher that enables Harmony 1.x and Harmony 2.x mods to coexist in Lobotomy Corporation.

## Problem

Lobotomy Corporation's mod loader (LMM/Basemod) uses Harmony 1.09, while BepInEx ships with HarmonyX (a Harmony 2 fork). Both versions use the assembly name `0Harmony`. In .NET Framework 3.5, assembly resolution works by name only, so when BepInEx loads its `0Harmony` first, LMM tries to use the wrong version and crashes.

BepInEx solves this by loading two DLLs:
- `0Harmony.dll` (Harmony 2 / HarmonyX)
- `0Harmony109.dll` (Harmony 1.09)

## Solution

`RetargetHarmony` runs as a BepInEx preloader patcher before the game loads. It uses Mono.Cecil to rewrite assembly references in `Assembly-CSharp.dll` and `LobotomyBaseModLib.dll` from `0Harmony` to `0Harmony109` in-memory. This lets LMM use the correct Harmony 1 DLL while Harmony 2 mods use the Harmony 2 DLL—without permanently modifying any files.

## Installation

### Build

```bash
dotnet build RetargetHarmony/RetargetHarmony.csproj
```

The compiled DLL will be at:
- `RetargetHarmony/bin/Debug/net35/RetargetHarmony.dll` (Debug)
- `RetargetHarmony/bin/Release/net35/RetargetHarmony.dll` (Release)

### Deploy

Copy the built DLL to your game's BepInEx installation:

```bash
# From the project root directory
cp RetargetHarmony/bin/Release/net35/RetargetHarmony.dll /path/to/LobotomyCorp/BepInEx/patchers/
```

### Uninstall

Simply delete the file:

```bash
rm /path/to/LobotomyCorp/BepInEx/patchers/RetargetHarmony.dll
```

## How It Works

1. BepInEx loads the preloader patcher from `BepInEx/patchers/RetargetHarmony.dll`
2. The patcher identifies target assemblies via the `TargetDLLs` property
3. For each target assembly, BepInEx calls the `Patch` method with an `AssemblyDefinition`
4. The patcher:
   - Finds all Harmony references (`0Harmony` or `0Harmony109`)
   - Ensures the first reference points to `0Harmony109`
   - Removes any duplicate Harmony references
5. BepInEx loads the patched assembly into the game

## Interaction with LMM/Basemod

LMM/Basemod expects to reference `0Harmony` (Harmony 1). After patching, the assembly references `0Harmony109` instead. At runtime, .NET Framework 3.5 resolves `0Harmony109` to the Harmony 1 DLL, satisfying LMM's expectations.

Harmony 2 mods continue to use `0Harmony`, which resolves to the HarmonyX DLL loaded by BepInEx.

## Testing

Run the test suite:

```bash
dotnet test RetargetHarmony.Test/RetargetHarmony.Test.csproj
```

Tests include:
- **Real-DLL tests**: Verify patcher works on actual game DLLs (`Assembly-CSharp.dll`, `LobotomyBaseModLib.dll`)
- **Synthetic tests**: Cover edge cases like no Harmony reference, idempotency, duplicate references, mixed references

## Technical Details

- **Target Framework**: .NET Framework 3.5 (to match the game)
- **Dependencies**:
  - BepInEx.Core 5.4.21
  - BepInEx.BaseLib 5.4.21
  - Mono.Cecil 0.10.4 (for assembly manipulation)
- **Coverage**: 100% (all branches tested)

## Logging

The patcher logs to BepInEx's logging system. Messages appear in:
- The BepInEx console window
- `BepInEx/LogOutput.log`

Example log output:
```
[Info:RetargetHarmony] Rewrote reference 0Harmony -> 0Harmony109 in Assembly-CSharp
[Info:RetargetHarmony] Rewrote reference 0Harmony -> 0Harmony109 in LobotomyBaseModLib
```

## License

SPDX-License-Identifier: MIT
