# Lobotomy Corporation Mods

Mods for the
game [Lobotomy Corporation](https://store.steampowered.com/app/568220/Lobotomy_Corporation__Monster_Management_Simulation/).
They are designed to be used either together or individually, so feel free to pick and choose.

All mods are available for download
on [my Nexus Mods page](https://next.nexusmods.com/profile/IsitChris/mods?gameId=2861). Requires
either [Lobotomy Mod Manager](https://www.nexusmods.com/site/mods/765)
or [Basemod](https://www.nexusmods.com/lobotomycorporation/mods/2) (included in LMM).

## Nexus Mods pages

- [Bad Luck Protection for Gifts](https://www.nexusmods.com/lobotomycorporation/mods/476) ([Readme](LobotomyCorporationMods.BadLuckProtectionForGifts/README.md))
- [Free Customization](https://www.nexusmods.com/lobotomycorporation/mods/477) ([Readme](LobotomyCorporationMods.FreeCustomization/README.md))
- [Unofficial Bug Fixes](https://www.nexusmods.com/lobotomycorporation/mods/478) ([Readme](LobotomyCorporationMods.BugFixes/README.md))
- [Warn When Agent Will Die From Working](https://www.nexusmods.com/lobotomycorporation/mods/479) ([Readme](LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking/README.md))

## Most Recent Change

### [4.1.0] - 2024-06-23

#### Added

- All mods
    - Added Info.xml file to add the version number and description in the in-game mod menu.

#### Fixed

- Warn When Agent Will Die From Working [1.1.0]
    - Nothing There
        - No longer checks for Justice below Level 4 (does not actually kill the agent).
        - Only checks for Fortitude below Level 4 if Nothing There is already disguised (before it would warn for
          Fortitude below Level 4 even if Nothing There was not disguised).
        - No longer always warns when Nothing There is disguised (Nothing There only kills while disguised if Fortitude
          is below Level 4).

See the [full changelog](CHANGELOG.md) for all changes.

## List of mods

### [Bad Luck Protection for Gifts](LobotomyCorporationMods.BadLuckProtectionForGifts/README.md)

![Bad Luck Protection for Gifts example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/bad-luck-protection.png)

A mod that provides increasing bad luck protection for agents that work on abnormalities to receive a gift from that
abnormality.
After an agent finishes their work, you can see the modified chance in the Abnormality Details screen at the E.G.O. Gift
section next to Acquisition Probability.

[Mod Readme](LobotomyCorporationMods.BadLuckProtectionForGifts/README.md) for full details.

---

### Free Customization

![Free Customization example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/free-customization.png)

Allows customizing (and re-customizing) agents without having to spend additional LOB points.

[Mod Readme](LobotomyCorporationMods.FreeCustomization/README.md) for full details.

---

### Unofficial Bug Fixes

A collection of bug fixes to fix various minor issues in the original game code.

Bugs fixed:

- Wasted stat upgrades from gifts with large negative stat minuses
- Crumbling Armor gift kills agents after being replaced

[Mod Readme](LobotomyCorporationMods.BugFixes/README.md) for full details.
 
---

### Warn When Agent Will Die From Working

![Warn When Agent Will Die From Working example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/warn-when-agent-will-die-from-working.png)

Provides a visual warning when an agent will be guaranteed to die to an instakill mechanic if assigned to work on an
abnormality.
The warning only appears when an abnormality has been fully observed to avoid spoiling newly-acquired abnormalities.

[Mod Readme](LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking/README.md) for full details.

---

## Building

The original game files are required but are not provided.
My current environment setup is a "`src`" folder in the BaseMods folder that I placed the repo in, so my folder
structure for the repo is LobotomyCorp_Data/BaseMods/src/lobotomy-corporation-mods.
If you follow this same structure then the references should use the game’s files and will build the output to the
appropriate BaseMod folder e.g. LobotomyCorp_Data/BaseMods/LobotomyCorporationMods.BadLuckProtectionForGifts.

If you’re running Linux, make sure mono-devel is installed.
You should be able to open the solution in VSCode or VSCodium and build with xbuild.
That said, I wouldn't recommend it as I have not found a way to be able to debug Unity .NET Framework DLLs in Linux like
I can in Windows with dnSpy, especially since the game has to run in Proton.
If you are able to get debugging working in Linux I would love to hear about it!

## Debug Logging

In the release versions errors are logged to a text file, but if you deploy as debug DLLs they will also appear in-game
in both the system log and as an Angela notification:

![Debug Logging example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/debug-logging.png)

## License

This work is licensed under MIT.

`SPDX-License-Identifier: MIT`
