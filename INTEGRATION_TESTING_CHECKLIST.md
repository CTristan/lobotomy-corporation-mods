# Integration Testing Checklist

A checklist for testing all the mods together in-game to verify that there are no incompatibilities and every mod works
as expected.

Note: this will contain spoilers.

## Bad Luck Protection For Gifts

- [ ] Creates BadLuckProtectionForGifts.dat file.
- [ ] Completing the day records the number of times worked for each abnormality and agent.
- [ ] Restarting the day doesn't add to number of times worked in dat file.
- [ ] Working, restarting, then working and completing a day doesn't include work counts from before the restart.
- [ ] Creating a new game empties the BadLuckProtectionForGifts.dat file.

## Unofficial Bugfixes

- [ ] Wasted Stat Upgrades
    - [ ] Have an agent with a base Fortitude of 65 that has Crumbling Armor's gift, causing the Fortitude level to be
      level 3. Upgrade the agent's Fortitude and verify that the Fortitude level is now level 4.
- [ ] Crumbling Armor Bugfix
    - [ ] Start a day with an agent that has Crumbling Armor's gift, replace Crumbling Armor's gift, then have the agent
      do attachment work and make sure the agent doesn't die.

## Free Customization

- [ ] Generate a new agent and make sure it doesn't cost any extra LOB to customize.
- [ ] Re-customize an existing agent and verify that customization doesn't cost LOB.
- [ ] Rename an agent to make sure changes stay.
- [ ] Re-customize an existing agent (including a name change), load the day, then restart the day and verify changes
  stay.

## Notify When Agent Receives Gift

- [ ] Displays a message for each condition:
    - [ ] Have an agent receive a gift in an empty slot.
    - [ ] Have an agent replace a gift in an existing slot.
- [ ] Does NOT display a message for each condition:
    - [ ] An agent would receive a gift when they already have that gift.
    - [ ] An agent would receive a gift when that slot is locked with another gift.

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
    - [ ] DEAD: Same agent works on Happy Teddy Bear twice in a row.
    - [ ] NOT dead: Different agent works on Happy Teddy Bear.
- [ ] Nothing There
    - [ ] DEAD: Agent has Fortitude below four while Nothing There is disguised.
    - [ ] NOT dead: Agent has Fortitude below four while Nothing There is not disguised.
    - [ ] NOT dead: Agent has Fortitude above three while Nothing There is disguised.
- [ ] Parasite Tree
    - [ ] DEAD: Agent does not have effect and Parasite Tree has four flowers.
    - [ ] NOT dead: Agent has effect and Parasite Tree has four flowers.
    - [ ] NOT dead: Agent does not have effect and Parasite Tree has flowers below four.
- [ ] Red Shoes
    - [ ] DEAD: Agent has Temperance below three.
    - [ ] NOT dead: Agent has Temperance above two.
- [ ] Spider Bud
    - [ ] DEAD: Agent has one Prudence and performs work other than Insight.
    - [ ] DEAD: Agent has Prudence above one and performs Insight work.
    - [ ] NOT dead: Agent has Prudence above one and performs work other than Insight.
- [ ] Singing Machine
    - [ ] DEAD: Qliphoth counter is zero.
    - [ ] DEAD: Agent has Fortitude above three and Temperance above two.
    - [ ] DEAD: Agent has Fortitude below four and Temperance below three.
    - [ ] DEAD: Agent has Fortitude from 57 to 64 and does not have gift.
    - [ ] NOT dead: Agent has Fortitude below four and Temperance above two.
- [ ] Void Dream
    - [ ] DEAD: Agent has one Temperance.
    - [ ] NOT dead: Agent has Temperance above one.
- [ ] Warm-Hearted Woodsman
    - [ ] DEAD: Qliphoth counter is zero.
    - [ ] NOT dead: Qliphoth counter is above zero.
