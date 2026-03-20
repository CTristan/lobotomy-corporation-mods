# DebugPanel

DebugPanel is a diagnostic overlay mod for Lobotomy Corporation that helps troubleshoot mod-related issues directly in-game.

## What it does

When installed, DebugPanel adds an in-game overlay (toggled with a hotkey) that displays:

- **Loaded mods** — lists all detected BaseMods and BepInEx plugins with their versions and Harmony version
- **Active patches** — shows all Harmony patches currently applied to game methods
- **Assembly info** — displays loaded assemblies and their references
- **DLL integrity** — checks whether mod DLLs have been properly retargeted by RetargetHarmony
- **Error logs** — surfaces errors from BepInEx, Unity, and gameplay logs
- **Known issues** — matches detected mods against a database of known compatibility problems
- **Filesystem validation** — verifies that expected game files and mod files are in place

## How to use

1. Click **Install** to copy DebugPanel to your game's `BaseMods/DebugPanel/` folder
2. Launch Lobotomy Corporation
3. Press the configured hotkey (default: **F12**) to toggle the diagnostic overlay
4. Use the overlay tabs to inspect different categories of diagnostic data

## When to use it

- Mods aren't loading or behaving unexpectedly
- You want to verify RetargetHarmony is working correctly
- You need to check which patches are active on a specific game method
- You want to share diagnostic information when reporting a bug
