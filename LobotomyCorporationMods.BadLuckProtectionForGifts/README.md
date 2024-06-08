# Bad Luck Protection for Gifts

![Bad Luck Protection for Gifts example](https://raw.githubusercontent.com/ctristan/lobotomy-corporation-mods/assets/bad-luck-protection.png)

## Overview

A mod that provides increasing bad luck protection for agents that work on abnormalities to receive a gift from that abnormality.
After an agent finishes their work, you can see the modified chance in the Abnormality Details screen at the E.G.O. Gift section next to Acquisition Probability.

## Rationale

As an agent works with an abnormality they should become more familiar with it, which in turn means they should be more likely to receive a gift from it.
It tracks the work count for each agent for each gift, so only the work that agent puts towards that gift will count.
Work counts are saved across days, but only if the day is completed; if the day is reset then all the work counts incremented during that day will reset as well and will go back to the numbers they were when the day was started.
It wouldn't make sense for an agent to remember what happened after resetting the day.

## Changelog

### [1.0.1] - 2020-10-13

#### Fixed

- Fixed infinite loop caused by finishing work on a creature with no gifts.

### [1.0] - 2020-10-09

#### Added

- Initial release.