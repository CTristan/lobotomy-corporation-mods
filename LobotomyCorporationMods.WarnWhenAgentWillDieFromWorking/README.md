# Warn When Agent Will Die From Working

![Warn When Agent Will Dire From Working example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/warn-when-agent-will-die-from-working.png)

## Overview

Provides a visual warning when an agent is guaranteed to die if assigned to work
on an abnormality. The warning only appears when an abnormality has been fully
observed to avoid spoiling newly acquired abnormalities.

## What does this mod do?

- Changes the work assignment UI for fully observed abnormalities so that any
  agents that would trigger an instant-kill mechanic show up in all-red text and
  display the "Dead" text when hovered over.
  - Note that this only takes effect if the abnormality is fully observed.

## Why create this mod?

Discovering how to deal with abnormalities is a major part of the game. However,
later on there are so many that have instant death abilities that are hard to
keep track of. It’s really frustrating to be almost done with a day with no
deaths, only to have an agent die because I forgot an instant death condition,
missed a temporary condition, or encountered a rare edge case.

Just to clarify, this is only for cases where the agent is guaranteed to die
because of an instant-kill mechanic, not because they would die from low
health/sanity or sending a low-chance agent would cause a bad result.

## Abnormality Warnings (Spoilers)

Provides warnings for the following abnormalities and any of their conditions (
*spoilers*):

- **Beauty and the Beast**
  - Assigning Repression work to Beauty and the Beast when the last work
    performed on Beauty and the Beast was Repression.
- **Bloodbath**
  - Agent has either Fortitude or Temperance Level 1.
- **Blue Star**
  - Agent has Prudence below Level 5 or Temperance below Level 4 (which would
    cause the work to take longer than 60 seconds).
- **Crumbling Armor**
  - Any of the following are true:
    - Agent has Fortitude Level 1
    - Assigning Attachment work (to any abnormality) while the agent has the
      Crumbling Armor gift.
- **Fairy Festival**
  - Assigning work to any other abnormality while the agent has the effect.
- **Happy Teddy Bear**
  - Re-assigning the agent that last worked with Happy Teddy Bear.
- **Laetitia**
  - Assigning work to any other abnormality while the agent has the effect.
- **Nothing There**
  - Agent has Fortitude below Level 4, while "Nothing There" is disguised.
- **Parasite Tree**
  - Assigning work when Parasite Tree has four bulbs and the agent doesn't have
    the effect (which would cause the fifth bulb to appear).
- **Red Shoes**
  - Agent has Temperance below Level 3.
- **Singing Machine**
  - Any of the following are true:
    - Qliphoth Counter is 0.
    - Agent has Temperance below Level 3.
    - Agent either has Fortitude above Level 3 or if the agent doesn't have the
      gift and receiving the gift would increase their Fortitude above Level 3,
      causing the agent to die upon receiving the gift.
- **Snow Queen**
  - Agent is wearing the Feather of Honor armor.
- **Spider Bud**
  - Any of the following are true:
    - Agent has Prudence Level 1.
    - Assigning Insight work to Spider Bud.
- **Void Dream**
  - Agent has Temperance Level 1.
- **Warm-Hearted Woodsman**
  - Qliphoth Counter is 0.

## Changelog

### [1.1.0] - 2024-06-23

#### Added

- Added Info.xml file to add the version number and description in the in-game
  mod menu.

#### Fixed

- Fixed Nothing There detection.
  - Removed check for Justice below Level 4 (does not kill the agent).
  - Limited Fortitude below Level 4 check to only trigger when "Nothing There"
    is already disguised.
  - Removed false warning when "Nothing There" is disguised but agent has
    Fortitude above Level 3.

### [1.0.0] - 2023-02-09

#### Added

- Initial release.
