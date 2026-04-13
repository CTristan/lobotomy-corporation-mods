# Integration Testing Checklist

A checklist for testing all the mods together in-game to verify that there are
no incompatibilities and every mod works as expected.

Note: this will contain spoilers.

## Bad Luck Protection For Gifts

- [ ] Creates BadLuckProtectionForGifts.dat file.
- [ ] Completing the day records the number of times worked for each abnormality
  and agent.
- [ ] Restarting the day doesn't add to the number of times worked in dat file.
- [ ] Working, restarting, starting, then completing a day doesn't include work
  counts from before the restart.
- [ ] Creating a new game empties the BadLuckProtectionForGifts.dat file.
- [ ] The .dat file uses V1 format with agent-specific tracking data.

### Configuration (requires ConfigurationManager)

- [ ] Press F1 to open ConfigurationManager. All settings for Bad Luck
  Protection For Gifts appear.
- [ ] Change Bonus Calculation Mode to "Per PE-Box" and verify the bonus
  increases by 1 per PE box instead of using the normalized ratio.
- [ ] Change a risk-level bonus percentage (e.g., ZAYIN to 5%) and verify the
  gift chance increases by 5% per work session for ZAYIN abnormalities.
- [ ] Set Reset On Gift Received to false. Receive a gift and verify the
  agent's bonus does not reset to zero.
- [ ] Set Gift Chance Decimal Places to 0 and verify the display shows whole
  numbers (e.g., "11%").
- [ ] Set Show Base Chance to false and verify the base chance is hidden from
  the display.

### Per-Agent Tracking

- [ ] Have two different agents work on the same abnormality. Verify that each
  agent's gift chance reflects only their own work count.
- [ ] Verify the display shows the agent's name (e.g., "BongBong Next
  Chance:11.20%").

### Without ConfigurationManager

- [ ] Remove ConfigurationManager and verify the mod still loads and works with
  default settings.

## Unofficial Bugfixes

- [ ] Wasted Stat Upgrades
  - [ ] Have an agent with a base Fortitude of 65 that has Crumbling Armor's
    gift, causing the Fortitude level to be level 3. Upgrade the agent's
    Fortitude and verify that the Fortitude level is now level 4.
- [ ] Crumbling Armor Bugfixes
  - [ ] Start a day with an agent that has Crumbling Armor's gift, replace
    Crumbling Armor's gift, then have the agent do attachment work and make sure
    the agent doesn't die.
  - [ ] Have an agent with multiple gifts get Crumbling Armor's gift, replace
    one of the other gifts, then have the agent do attachment work and make sure
    the agent dies.

## Free Customization

- [ ] Generate a new agent and make sure it doesn't cost any extra LOB to
  customize.
- [ ] Re-customize an existing agent and verify that customization doesn't cost
  LOB.
- [ ] Rename an agent to make sure changes stay.
- [ ] Re-customize an existing agent (including a name change), load the day,
  then restart the day, and verify changes stay.

## Gift Alert Icon

- [ ] A green gift icon appears if the slot is empty.
- [ ] A gray gift icon appears if the agent has a gift in that slot but not that
  specific gift.
- [ ] No gift icon appears if the agent already has that gift.

## Notify When Agent Receives Gift

- [ ] Displays a message for each condition:
  - [ ] Have an agent receive a gift in an empty slot.
  - [ ] Have an agent replace a gift in an existing slot.
- [ ] Doesn't display a message for each condition:
  - [ ] An agent would receive a gift when they already have that gift.
  - [ ] An agent would receive a gift when that slot is locked with another
    gift.
  - [ ] Agent receives Snow Queen's icicle from a normal/bad work result.

## Warn When Agent Will Die From Working

- [ ] Beauty and the Beast
  - [ ] DEAD: Perform Repression work while weakened.
  - [ ] NOT dead: Perform Repression work while not weakened.
  - [ ] NOT dead: Perform non-Repression work while weakened.
- [ ] Bloodbath
  - [ ] DEAD: Agent has one Fortitude and Temperance above one.
  - [ ] DEAD: Agent has one Temperance and Fortitude above one.
  - [ ] NOT dead: Agent has both Fortitude and Temperance above one.
- [ ] Blue Star
  - [ ] DEAD: Agent has Prudence below five and Temperance above three.
  - [ ] DEAD: Agent has five Prudence and Temperance below four.
  - [ ] NOT dead: Agent has five Prudence and Temperance above three.
- [ ] Crumbling Armor
  - [ ] DEAD: Agent has one Fortitude.
  - [ ] DEAD: Agent with gift performs Attachment work.
  - [ ] NOT dead: Agent has Fortitude above one.
- [ ] Fairy Festival
  - [ ] DEAD: Agent with effect works on another abnormality.
  - [ ] NOT dead: Agent with effect works on Fairy Festival.
- [ ] Laetitia
  - [ ] DEAD: Agent with effect works on another abnormality.
  - [ ] NOT dead: Agent with effect works on Laetitia.
- [ ] Happy Teddy Bear
  - [ ] DEAD: The same agent works on Happy Teddy Bear twice in a row.
  - [ ] NOT dead: Different agent works on Happy Teddy Bear.
- [ ] Nothing There
  - [ ] DEAD: Agent has Fortitude below four while "Nothing There" is disguised.
  - [ ] NOT dead: Agent has Fortitude below four while "Nothing There" is not
    disguised.
  - [ ] NOT dead: Agent has Fortitude above three while Nothing There is
    disguised.
- [ ] Parasite Tree
  - [ ] DEAD: Agent doesn't have the effect and Parasite Tree has four flowers.
  - [ ] NOT dead: Agent has the effect and Parasite Tree has four flowers.
  - [ ] NOT dead: Agent doesn't have the effect, and Parasite Tree has flowers
    below four.
- [ ] Red Shoes
  - [ ] DEAD: Agent has Temperance below three.
  - [ ] NOT dead: Agent has Temperance above two.
- [ ] Singing Machine
  - [ ] DEAD: Qliphoth counter is zero.
  - [ ] DEAD: Agent has Fortitude above three and Temperance above two.
  - [ ] DEAD: Agent has Fortitude below four and Temperance below three.
  - [ ] DEAD: Agent has Fortitude from 57 to 64 and doesn't have the gift.
  - [ ] NOT dead: Agent has Fortitude below four and Temperance above two.
- [ ] Snow Queen
  - [ ] DEAD: Agent has the Feather of Honor armor equipped.
  - [ ] NOT dead: Agent doesn't have the Feather of Honor equipped.
- [ ] Spider Bud
  - [ ] DEAD: Agent has one Prudence and performs work other than Insight.
  - [ ] DEAD: Agent has Prudence above one and performs Insight work.
  - [ ] NOT dead: Agent has Prudence above one and performs work other than
    Insight.
- [ ] Void Dream
  - [ ] DEAD: Agent has one Temperance.
  - [ ] NOT dead: Agent has Temperance above one.
- [ ] Warm-Hearted Woodsman
  - [ ] DEAD: Qliphoth counter is zero.
  - [ ] NOT dead: Qliphoth counter is above zero.
