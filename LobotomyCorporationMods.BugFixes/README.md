# Unofficial Bug Fixes

A collection of bug fixes to fix various minor issues in the original game code.

Bugs fixed:

- **Wasted stat upgrades**
  - If a gift provides a large negative stat bonus (for example, Crumbling Armor
    -20 HP), then when upgrading that stat, it would incorrectly treat the
    upgrade as a lower level and not improve it. For example, when an agent had
    base level 4 Fortitude but had the Crumbling Armor gift with -20 HP, it
    would have a modified Fortitude of level 3. However, the upgrade would use
    the modified level instead of the base level, so Fortitude would remain
    level 3 after the upgrade instead of increasing by 1 (base level 5, modified
    level 4).
  - This bug fix ensures that the game uses the base level for upgrades instead
    of the modified level.
- **Crumbling Armor gift kills agents after being replaced**
  - When an agent that started the day with Crumbling Armor's gift later
    replaced the gift with another one, they could still die when performing an
    Attachment work.
  - This bug fix ensures that agents no longer die from Crumbling Armor's effect
    after replacing the gift with another one.
- **Crumbling Armor gift does not kill the agent when another gift is replaced**
  - When an agent with Crumbling Armor's gift replaces a gift in another slot
    like Hand or Face, they would no longer die from Crumbling Armor's effect
    for doing Attachment work.
  - This bug fix ensures that agents will now correctly die from Crumbling
    Armor's effect after replacing a gift in a separate slot.

## Changelog

### [3.0.1] - 2024-08-07

#### Fixed

- Fixed a potential game slowdown when finishing work with Crumbling Armor.

### [3.0.0] - 2024-07-04

#### Added

- Added bug fix for Crumbling Armor not killing agents that replaced a
  different gift.

### [2.1.0] - 2024-06-23

#### Added

- Added Info.xml file to add the version number and description in the in-game
  mod menu.

### [2.0.0] - 2023-02-01

#### Added

- Added bug fix for Crumbling Armor killing agents when they no longer had
  Crumbling Armor's gift.

### [1.0.0] - 2023-01-18

#### Added

- Initial release.
  - Fixed a game bug that could cause LOB points to be wasted with certain
    gifts.
