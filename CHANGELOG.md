# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

The LobotomyCorporationMods.Common project has a custom versioning system
explained below, but each mod adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) unless spec.

## Common-library version numbers

The LobotomyCorporationMods.Common library project uses the following custom
versioning:

Major.minor.patch.0

- Major version is the total number of mods created.
- Minor version is if there are new additions to existing mods.
- Patch version is for bug fixes.
- The fourth value is for development purposes only.

*This does not apply to the individual mod versions, as they adhere
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).*

## [6.0.2] - 2024-08-06

### Fixed

- Unofficial Bug Fixes [3.0.1]
  - Fixed a potential game slowdown when finishing work with Crumbling Armor.

## [6.0.1] - 2024-07-15

### Added

- Gift Alert Icon [1.0.1]
  - Added Chinese localization thanks to 21474836(Lin).

## [6.0.0] - 2024-07-14

### Added

- New mod: Gift Alert Icon [1.0.0]
  - Shows an icon when a new or replacement gift is available for an agent.
- Notify When Agent Receives Gift [1.1.0]
  - Added localization through external text files.
  - Added Chinese localization thanks to 21474836(Lin).

## [5.0.0] - 2024-07-04

### Added

- New mod: Notify When Agent Receives Gift [1.0.0]
  - Whenever an agent receives a gift, a message will be displayed in the log.
- Unofficial Bug Fixes [3.0.0]
  - Added fix for Crumbling Armor bug where replacing ANY gift would cause the
    agent to no longer die from doing Attachment work.
- Warn When Agent Will Die From Working [15.0.0]
  - Added check for Snow Queen when the agent is wearing the Feather of Honor
    armor.

## [4.1.0] - 2024-06-23

### Added

- All mods
  - Added Info.xml file to add the version number and description in the in-game
    mod menu.

### Fixed

- Warn When Agent Will Die From Working [14.1.0]
  - Nothing There
    - No longer checks for Justice below Level 4 (does not kill the agent).
    - Only checks for Fortitude below Level 4 if "Nothing There" is already
      disguised (before it would warn for Fortitude below Level 4 even if
      "Nothing There" was not disguised).
    - No longer always warns when "Nothing There" is disguised ("Nothing There"
      only kills while disguised if Fortitude is below Level 4).

## [4.0.1] - 2023-02-10

### Fixed

- Free Customization [2.1]
  - Fixed the issue with renames not working for the initial starting agent when
    starting from Day 1.

## [4.0.0] - 2023-02-09

### Added

- New mod: Warn When Agent Will Die From Working [14.0.0]
  - Warns when assigning work to an agent who will die from an instant-kill
    mechanic.
  - More information is available
    in [the mod's readme](LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking/README.md).

## [3.2.0] - 2023-02-01

### Added

- Unofficial Bug Fixes [2.0.0]
  - Added bug fix for Crumbling Armor killing agents when they no longer had
    Crumbling Armor's gift.

## [3.1.0] - 2023-01-30

### Added

- Free Customization [2.0.0]
  - Added re-customization capability to Free Customization mod.

## [3.0.0] - 2023-01-18

### Added

- New mod: Unofficial Bug Fixes [1.0.0]
  - Fixes a game bug that could cause LOB points to be wasted with certain
    gifts.
  - More information is available
    in [the mod's readme](LobotomyCorporationMods.BugFixes/README.md).

## [2.0.0] - 2022-07-25

### Added

- New mod: Free Customization [1.0.0]
  - Customizing an agent no longer costs any LOB points.
  - More information is available
    in [the mod's readme](LobotomyCorporationMods.FreeCustomization/README.md).

## [1.0.1] - 2020-10-13

### Fixed

- Bad Luck Protection for Gifts [1.0.1]
  - Finishing work on a creature with no gifts caused an infinite loop.

## [1.0.0] - 2020-10-09

### Added

- First mod!
  - Bad Luck Protection for Gifts [1.0.0]
  - The more an agent works with an abnormality, the higher chance for them to
    receive their gift.
  - More information is available
    in [the mod's readme](LobotomyCorporationMods.BadLuckProtectionForGifts/README.md).
