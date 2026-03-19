# User's Guide

## What is Harmony 2 for LMM?

Harmony 2 for LMM is a compatibility layer that lets **Harmony 2** mods run alongside
the game's existing **Harmony 1.x** mod system (LMM — Lobotomy Mod Manager).

Lobotomy Corporation ships with Harmony 1.x built in. Some newer mods are written
using Harmony 2, which offers more features and better stability. This installer
bridges the gap so both types of mods can coexist.

## Do I need this?

You need Harmony 2 for LMM if:

- You want to use a mod that **requires Harmony 2** (the mod's description will say so)
- You are a modder who wants to use **Harmony 2 or BepInEx** features

If all your mods work fine without it, you don't need to install anything.

## How does it work?

The installer deploys two components into your game directory:

1. **BepInEx 5.4.23.5** — a modding framework that loads before the game starts.
   It runs transparently and does not interfere with existing mods.
2. **RetargetHarmony** — a BepInEx preloader patcher that rewrites mod assembly
   references at load time, so Harmony 1.x mods and Harmony 2 mods can both find
   the APIs they expect.

When the game starts, BepInEx loads first, RetargetHarmony patches assembly references,
and then LMM loads your mods as usual. The entire process is automatic.

## Where do mods go?

**All mods — both Harmony 1 and Harmony 2 — still go in the `BaseMods/` folder**,
exactly the same as before. You do not need a separate folder for Harmony 2 mods.

## Installation states explained

When you point the installer at your game directory, it detects one of these states:

| State | Meaning |
|-------|---------|
| **Fresh** | Harmony 2 for LMM is not installed. Click **Install** to set it up. |
| **Current** | The installed version matches this installer. Everything is up to date. You can **Reinstall** if needed. |
| **Outdated** | An older version is installed. Click **Upgrade** to update to the latest version. |
| **Newer** | A *newer* version is installed (you may be running an older installer). **Downgrading** is possible but not recommended. |
| **Corrupted** | Some installed files are missing or damaged. The missing files are listed. Click **Repair** to restore them. |

## Warnings explained

### Flagged mods

If the installer detects mods in your `BaseMods/` folder that directly reference
BepInEx or Harmony 2 assemblies, it will list them as **dependent mods**. This is
informational — these mods rely on Harmony 2 for LMM and will stop working if you
uninstall it.

### Version warnings

If a newer version is already installed, the installer shows a downgrade warning.
Proceeding will replace the newer installation with this installer's version.

## Troubleshooting

### Game won't start after installation

1. Verify the game directory path is correct
2. Try **Repair** (or **Reinstall**) from the installer
3. Check `BepInEx/LogOutput.log` in your game directory for error messages
4. As a last resort, **Uninstall** to remove all Harmony 2 for LMM files and restore
   the game to its original state

### Mods not loading

1. Confirm your mods are in the `BaseMods/` folder
2. Check `BepInEx/LogOutput.log` for errors related to your mod
3. Make sure the mod is compatible with your game version

### Where are the logs?

BepInEx writes a log file to `BepInEx/LogOutput.log` inside your game directory.
This file is recreated each time the game starts and contains detailed information
about what loaded (and what failed).

## Uninstalling

The **Uninstall** button removes all files that were installed by Harmony 2 for LMM,
including:

- BepInEx framework files
- RetargetHarmony patcher
- Harmony interop DLLs
- The `.harmony2forlmm/` manifest directory

**Your mods in `BaseMods/` are never removed.** However, mods that depend on Harmony 2
will no longer work after uninstalling. The installer warns you about these mods before
you proceed.
