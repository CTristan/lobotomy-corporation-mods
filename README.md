# Lobotomy Corporation Mods

Mods for the
game [Lobotomy Corporation](https://store.steampowered.com/app/568220/Lobotomy_Corporation__Monster_Management_Simulation/)
. They are designed to be used either together or individually, so feel free to
pick and choose.

Requires [Basemod](https://www.nexusmods.com/lobotomycorporation/mods/2).

## Table of Contents

* [Recent Changes](#recent-changes)
* [List of mods](#list-of-mods)
* [Change Log](#change-log)
* [Building](#building)
* [License](#license)

## Recent Changes

* 2023-01-29 (v3.1) - Added re-customization capability to Free Customization
  mod

* 2023-01-18 (v3.0) - New mod: Unofficial Bug Fixes

## List of mods

### Bad Luck Protection for Gifts

***
![Bad Luck Protection for Gifts example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/bad-luck-protection.png)

A mod that provides increasing bad luck protection for agents that work on
abnormalities to receive a gift from that abnormality. After an agent finishes
their work, you can see the modified chance in the Abnormality Details screen at
the E.G.O. Gift section next to Acquisition Probability.

The rationale is that as an agent works with an abnormality they become more
familiar with it, which in turn means they should be more likely to receive a
gift from it. It tracks the work count for each agent for each gift, so only the
work that agent puts towards that gift will count. Work counts are saved across
days, but only if the day is completed; if the day is reset then all the work
counts incremented during that day will reset as well and will go back to the
numbers they were when the day was started. It wouldn't make sense for an agent
to remember what happened after the day is reset.

### Free Customization

***
![Free Customization example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/free-customization.png)

Allows customizing (and re-customizing) agents without having to spend
additional LOB points.

I do not like when games require a cost for customizing a character in the first
place, but in this game customizing requires spending an additional LOB point
which you only get a limited number of each day and is used to hire agents and
improve stats. The fact that customizing an agent directly affects your gameplay
bothers me, so this mod will make customizing agents not require any additional
points.

### Unofficial Bug Fixes

***
A collection of bug fixes to fix various minor issues in the original game code.

Bugs fixed:

* Wasted stat upgrades - If a gift provides a large negative stat bonus (e.g.
  Crumbling Armor -20 HP), then when upgrading that stat it would incorrectly
  treat the upgrade as a lower level and not actually improve it. For example,
  when an agent had base level 4 Fortitude but had the Crumbling Armor gift with
  -20 HP it would have a modified Fortitude of level 3, and the upgrade would
  use the modified level instead of the base level so Fortitude would remain
  level 3 after the upgrade instead of increasing by 1 (base level 5, modified
  level 4).

## Change Log

* v3.1 - Added re-customization capability to Free Customization mod
* v3.0 - New mod: Unofficial Bug Fixes
* v2.0 - New mod: Free Customization
* v1.0.1 - Fixed an issue with Bad Luck Protection for Gifts
* v1.0 - First mod: Bad Luck Protection for Gifts

## Building

The original game files are required which are not provided. My current
environment setup is a "src" folder in the BaseMods folder that I placed the
repo in, so my folder structure for the repo is
LobotomyCorp_Data/BaseMods/src/lobotomy-corporation-mods. If you follow this
same structure then the references should use the game's files and will build
the output to the appropriate BaseMod folder e.g.
LobotomyCorp_Data/BaseMods/LobotomyCorporationMods.BadLuckProtectionForGifts.

When you build, you may get a build error saying that a metadata file for "
LobotomyCorporationMods.Common.########.dll" could not be found. All you need to
do is clean and rebuild the Common project. This happens because of a
customization to the build process that appends the build date to the Common
project DLL. I did this because BaseMod will try to re-use existing files with
the same name, so whenever the Common project is updated to add/fix something
for one mod it would break the loading process for another mod.

If you're running Linux, make sure mono-devel is installed. You should be able
to open the solution in VSCode or VSCodium and build with xbuild. That said, I
wouldn't recommend it as I have not found a way to be able to debug Unity .NET
Framework DLLs in Linux like I can in Windows with dnSpy, especially since the
game has to run in Proton. If you are able to get debugging working in Linux I
would love to hear about it!

## License

This work is licensed under MIT.

`SPDX-License-Identifier: MIT`
