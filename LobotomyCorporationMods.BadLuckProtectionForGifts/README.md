# Bad Luck Protection for Gifts

![Bad Luck Protection for Gifts example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/bad-luck-protection.png)

## Overview

A mod that provides increasing bad luck protection for agents that work on
abnormalities to receive a gift from that
abnormality.

## What does this mod do?

- Tracks the work count for each agent for each gift, so only the work that
  agent puts towards that gift will count.
- After an agent finishes their work, you can see the modified chance in the
  Abnormality Details screen at
  the E.G.O. Gift section next to Acquisition Probability.
- Work counts are saved across days, but only if the day is completed.
  - If the day is reset then all the work counts incremented during that day
    will reset as well and will go back to
    the numbers they were from when the day was started.
- When starting a new game from Day 1, resets all tracked counts and starts with
  a new tracker.

## Why create this mod?

I like the concept of gifts as a gameplay mechanic, but the implementation
creates WAY too much of a grind. The
percentages are extremely low and completely unrelated to how well the agent
did, which means an agent could be
constantly working with the same abnormality all day and never receive the gift.

This mod adds a bit of inevitability without making it immediately guaranteed,
and the better the success is the higher
the chance. As an agent works with an abnormality they should become more
familiar with it, which in turn means they
should be more likely to receive a gift from it.

## Changelog

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
