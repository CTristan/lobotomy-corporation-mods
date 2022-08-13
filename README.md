# Lobotomy Corporation Mods

Mods for the game
[Lobotomy Corporation](https://store.steampowered.com/app/568220/Lobotomy_Corporation__Monster_Management_Simulation/)
designed to work either together or individually.

Requires [Basemod](https://www.nexusmods.com/lobotomycorporation/mods/2).

## List of mods

### Bad Luck Protection for Gifts

![Bad Luck Protection for Gifts example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/bad-luck-protection.png)

A mod that provides increasing bad luck protection for agents that work on
abnormalities to receive a gift from that abnormality. After an agent finishes
their work, you can see the modified chance in the Abnormality Details screen at
the E.G.O. Gift section next to Acquisition Probability.

The rationale is that as an agent works with an abnormality they become more
familiar with it, which in turn means it should be easier for them to receive a
gift from it. It tracks the work count for each agent for each gift, so only the
work that agent puts towards that gift will count. The tracker saves work counts
across days when the day ends; if the day is reset then all the work counts
incremented during that day will reset as well and will go back to the numbers
they were when the day started. It wouldn't make sense for an agent to remember
what happened after the day is reset.

### Free Customization

![Free Customization example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/free-customization.png)

I do not like when games require a cost for customizing a character in the first
place, but in this game customizing requires spending another LOB point which
you get a limited number of each day and also needed to hire agents and improve
stats. The fact that customizing an agent directly affects your gameplay bothers
me, so this mod will make customizing agents not require more points.

## Building

The solution requires the original game files which are not provided. My current
environment setup is a "src" folder in the BaseMods folder that I placed the
repo in, so my folder structure for the repo is
LobotomyCorp_Data/BaseMods/src/lobotomy-corporation-mods. If you follow this
same structure then the references should use the game's files and will build
the output to the appropriate BaseMod folder e.g.
LobotomyCorp_Data/BaseMods/LobotomyCorporationMods.BadLuckProtectionForGifts.

If you're running Linux, you may need to install mono-devel. You should be able
to open the solution in VSCode or VSCodium and build with xbuild.

## Development Notes

* Whenever we make ANY changes to the Common project in ANY way, we need to
  change the Assembly Name for the Common library. This is because when Basemod
  loads the mods, if it finds two DLLs with the same assembly name then it will
  try to re-use the first DLL even if the second DLL is different, which causes
  the game to crash spectacularly. Since the version number doesn't matter I've
  been appending the date whenever I update the project.

## License

This work is licensed under MIT.

`SPDX-License-Identifier: MIT`
