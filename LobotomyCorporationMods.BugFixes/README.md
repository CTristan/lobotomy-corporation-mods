# Unofficial Bug Fixes

A collection of bug fixes to fix various minor issues in the original game code.

Bugs fixed:

- **Wasted stat upgrades** - If a gift provides a large negative stat bonus (e.g. Crumbling Armor -20 HP), then when upgrading that stat it would incorrectly treat the upgrade as a lower level and not actually improve it.
For example, when an agent had base level 4 Fortitude but had the Crumbling Armor gift with -20 HP it would have a modified Fortitude of level 3, and the upgrade would use the modified level instead of the base level so Fortitude would remain level 3 after the upgrade instead of increasing by 1 (base level 5, modified level 4).
- **Crumbling Armor gift kills agents after being replaced** - When an agent that started the day with Crumbling Armor’s gift later replaced the gift with another one, they would still die when performing an Attachment work.
