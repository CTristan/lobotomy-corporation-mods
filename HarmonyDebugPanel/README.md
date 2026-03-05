# HarmonyDebugPanel

HarmonyDebugPanel is a BepInEx 5 plugin for diagnosing Harmony usage in Lobotomy Corporation.

## What it does

- Logs a full Harmony diagnostic report on startup (`BepInEx/LogOutput.log`)
- Adds an in-game overlay (default hotkey: `F9`)
- Supports on-demand refresh (default hotkey: `F10`)

The report includes:

- Loaded BepInEx plugins (name, version, GUID, Harmony version)
- Loaded LMM/Basemod mods (name, version, Harmony version)
- RetargetHarmony detection and patch status
- Active Harmony patches
- Loaded assemblies with Harmony-related highlighting
- Collection warnings/errors

## Installation

1. Build the project:

```bash
dotnet build HarmonyDebugPanel/HarmonyDebugPanel.csproj -c Release
```

2. Copy the plugin DLL to your game:

```bash
cp HarmonyDebugPanel/bin/Release/net35/HarmonyDebugPanel.dll /path/to/LobotomyCorp/BepInEx/plugins/
```

## Configuration

Configuration file is generated at:

`BepInEx/config/com.ctristan.harmonydebugpanel.cfg`

Settings:

- `OverlayToggleHotkey` (default `F9`)
- `RefreshHotkey` (default `F10`)
- `ShowBepInExPlugins` (default `true`)
- `ShowLmmMods` (default `true`)
- `ShowActivePatches` (default `true`)
- `ShowAssemblyInfo` (default `true`)

If ConfigurationManager is installed, these settings appear in its UI automatically.

## Relationship to RetargetHarmony

HarmonyDebugPanel is a companion diagnostic tool. It does **not** patch assemblies itself.
It reports whether RetargetHarmony appears loaded and whether target assemblies look retargeted.

## Testing

```bash
dotnet test HarmonyDebugPanel.Test/HarmonyDebugPanel.Test.csproj
```
