# Lobotomy Corporation Mods

Mods for the
game [Lobotomy Corporation](https://store.steampowered.com/app/568220/Lobotomy_Corporation__Monster_Management_Simulation/)
. They are designed to be used either together or individually, so feel free to
pick and choose.

Requires [Basemod](https://www.nexusmods.com/lobotomycorporation/mods/2).

## List of mods

### Bad Luck Protection for Gifts

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

![Free Customization example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/free-customization.png)

I do not like when games require a cost for customizing a character in the first
place, but in this game customizing requires spending an additional LOB point
which you only get a limited number of each day and is used to hire agents and
improve stats. The fact that customizing an agent directly affects your gameplay
bothers me, so this mod will make customizing agents not require any additional
points.

## Building

The original game files are required which are not provided. My current
environment setup is a "src" folder in the BaseMods folder that I placed the
repo in, so my folder structure for the repo is
LobotomyCorp_Data/BaseMods/src/lobotomy-corporation-mods. If you follow this
same structure then the references should use the game's files and will build
the output to the appropriate BaseMod folder e.g.
LobotomyCorp_Data/BaseMods/LobotomyCorporationMods.BadLuckProtectionForGifts.

If you're running Linux, make sure mono-devel is installed. You should be able
to open the solution in VSCode or VSCodium and build with xbuild.

## License

This work is licensed under MIT.

`SPDX-License-Identifier: MIT`
