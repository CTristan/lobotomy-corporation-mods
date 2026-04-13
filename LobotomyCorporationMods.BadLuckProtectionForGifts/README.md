# Bad Luck Protection for Gifts

<!-- TODO: Update screenshot to show new display format with agent name and base chance -->
![Bad Luck Protection for Gifts example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/bad-luck-protection.png)

## Overview

A mod that increases the gift chance for agents as they work on abnormalities.
The bonus is tracked per agent and can be customized through in-game settings.

## What does this mod do?

- Tracks the work count for each agent for each gift. Each agent builds their
  own bonus independently.
- After an agent finishes work, the gift chance display shows the agent's name
  and their boosted probability. For example: `BongBong Next Chance:11.20%`
- The base gift chance can also be shown alongside the boosted chance. For
  example: `(Base:2.00%)`
- Work counts are saved across days, but only if the day is completed.
  - If the day is reset, then all the work counts incremented during that day
    will reset as well and will go back to the numbers they were from when the
    day was started.
- When starting a new game from Day 1, resets all tracked counts and starts with
  a new tracker.

## Configuration

Settings can be changed in-game if
[ConfigurationManager](https://github.com/ctristan/LobCorp.ConfigurationManager)
is installed. Press F1 to open the settings window. If ConfigurationManager is
not installed, the mod uses default values for all settings.

### General Settings

| Setting | Default | Description |
|---|---|---|
| Bonus Calculation Mode | Normalized | How the bonus is calculated. **Normalized**: bonus is based on filled PE boxes divided by total PE boxes, so all abnormalities gain bonus at the same rate. **Per PE-Box**: each filled PE box adds 1 to the bonus directly. |
| Reset On Gift Received | On | When an agent receives a gift, their bonus for that gift resets to zero. |
| Gift Chance Decimal Places | 2 | Number of decimal places (0 to 3) shown in the gift chance display. |
| Show Base Chance | On | Show the original gift chance next to the boosted chance. |

### Gift Chance Bonus Settings

These settings control how much extra gift chance is added after each successful
work session, based on the abnormality's risk level.

| Setting | Default | Range |
|---|---|---|
| ZAYIN Bonus Percentage | 1% | 0% to 100% |
| TETH Bonus Percentage | 1% | 0% to 100% |
| HE Bonus Percentage | 1% | 0% to 100% |
| WAW Bonus Percentage | 1% | 0% to 100% |
| ALEPH Bonus Percentage | 1% | 0% to 100% |

## Why create this mod?

I like the concept of gifts as a gameplay mechanic, but the implementation
creates WAY too much of a grind. The percentages are extremely low and
completely unrelated to how well the agent did, which means an agent could be
constantly working with the same abnormality all day and never receive the gift.

This mod adds a bit of inevitability without making it immediately guaranteed,
and the better the success is, the higher the chance. As an agent works with an
abnormality, they should become more familiar with it, which in turn means they
should be more likely to receive a gift from it.

## Changelog

### [1.2.0] - 2026-04-12

#### Added

- Added configuration support through ConfigurationManager (optional).
  - Added bonus calculation mode: Normalized (bonus based on filled PE boxes
    divided by total PE boxes) or Per PE-Box (each filled PE box adds 1 to the
    bonus).
  - Added adjustable bonus percentage per risk level (ZAYIN, TETH, HE, WAW,
    ALEPH).
  - Added option to reset bonus when an agent receives a gift.
  - Added configurable decimal places (0 to 3) for the gift chance display.
  - Added option to show or hide the base gift chance.
- Added agent name to the gift chance display.
- Added base gift chance alongside the boosted chance.

#### Changed

- Updated work tracker file format to V1. The mod can still read the old
  format.

#### Fixed

- Fixed a bug where the last agent's work count leaked into other agents' gift
  chance display.

### [1.1.0] - 2024-06-23

#### Added

- Added Info.xml file to add the version number and description in the in-game
  mod menu.

### [1.0.1] - 2020-10-13

#### Fixed

- Fixed infinite loop caused by finishing work on a creature with no gifts.

### [1.0.0] - 2020-10-09

#### Added

- Initial release.
