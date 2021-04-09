# Lobotomy Corporation Mods
Mods for the game [Lobotomy Corporation](https://store.steampowered.com/app/568220/Lobotomy_Corporation__Monster_Management_Simulation/). They are designed to be used either together or individually, so feel free to pick and choose.

## List of mods
### Bad Luck Protection for Gifts
A mod that provides increasing bad luck protection for agents that work on abnormalities to receive a gift from that abnormality. The rationale is that as an agent works with an abnormality they become more familiar with it, which in turn means they should be more likely to receive a gift from it. It tracks the work count for each agent for each gift, so only the work that agent puts towards that gift will count. Work counts are saved across days, but only if the day is completed; if the day is reset then all the work counts incremented during that day will reset as well and will go back to the numbers they were when the day was started. It wouldn't make sense for an agent to remember what happened after the day is reset.

## Building
The original game files are required which are not provided. My current environment setup is a "src" folder in the BaseMods folder that I placed the repo in, so my folder structure for the repo is LobotomyCorp_Data/BaseMods/src/lobotomy-corporation-mods. If you follow this same structure then the references should use the game's files and will build the output to the appropriate BaseMod folder e.g. LobotomyCorp_Data/BaseMods/LobotomyCorporationMods.BadLuckProtectionForGifts.

## License

This work is licensed under MIT.

`SPDX-License-Identifier: MIT`
