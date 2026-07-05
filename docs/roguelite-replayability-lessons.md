# Lobotomy Corporation — Cross-Game Replayability Lessons

> **What this is.** The distilled-meaning layer of the game-research pipeline: the per-game
> notebooks (`docs/game-research/`) hold raw findings, this doc holds the cross-game design
> principles those findings teach, and the master map (`docs/roguelite-replayability-approaches.md`)
> holds the scored applicability of each game. One rule keeps the layers from competing: **the map
> may name a principle; only this doc may argue one.** The map's `Design principles` list stays the
> one-line rubric of record, every principle's mechanism, evidence, boundary, and mod implication
> lives here, and a principle graduates into the map's list only through
> `/lobcorp-replayability-eval`, logged in the map's Decision log.
>
> **Living document.** Entries land and sharpen through `/lobcorp-game-debrief`. A twin copy lives
> in the Obsidian vault, so read **Syncing** at the end before you edit either one.

**How to read an entry.** Each lesson is one heading stating the principle as a falsifiable claim,
followed by the mechanism (why it works), the per-game evidence (honest about firsthand versus
secondhand), the boundary (what breaks it), which of the map's six approaches it feeds, and one
concrete implication for the mod. The maturity marker is mechanical: *(provisional)* under two
independent game citations, *(established)* at two or more. One mechanism, one entry: a re-taught
lesson sharpens an existing entry rather than adding a near-duplicate, and a new entry is warranted
only when the "because" paragraph would differ. Within a theme, established entries come before
provisional ones, then by descending evidence count. No scores here, because scores are the map's
vocabulary.

## Variance

Where imposed, recombinant variance comes from, because replay value starts with the system handing
you a different problem.

### Variance must be imposed, not chosen *(established)*

When the player steers the randomness, they steer toward the known-optimal line, so chosen variance
converges back onto the same run; an imposed different problem is the only thing that forces a
genuine re-plan.

- **Evidence:** Vanilla LobCorp — the draft-of-three abnormality offering is a choice, so every
  campaign lands on the same end-state (master map, The problem; firsthand). Against the Storm —
  every settlement rolls a biome, starting goods, and a blueprint pool, imposed conditions that
  force drafting differently than you would by default (notebook Mechanic inventory, 2026-07-01;
  firsthand verification pending). Into the Breach — islands, squads, and objectives vary by
  imposition, not selection (master map, Approach 5; secondhand).
- **Boundary:** it breaks the moment the player can freely reroll or veto the imposition, because
  a veto turns the imposition back into a choice and the convergence returns.
- **Feeds:** Approaches 2 (procedural/affixed content), 5 (structural run-restructuring).
- **For the mod:** the roguelite's variance sources (affix rolls, starting facility conditions,
  ordeal slates) get handed to the player, with mitigation earned in-run, never offered as a pick
  list.

### A few deeply distinct starts beat many shallow ones *(established)*

A start earns replay value by changing the decision loop from turn one, because a stat-swap start
converges back onto the same play patterns; four genuinely different kinds generate more distinct
runs than twenty variants.

- **Evidence:** Slay the Spire — four characters, each a different kind of run through a distinct
  starting deck, relic, and mechanic (notebook Approach mapping; master map, Approach 3;
  secondhand). Hades — aspects re-skin one weapon into fresh playstyles, many starts from few base
  kinds cheaply (master map, Approach 3; secondhand). dotAGE — an Elder reshapes the whole run's
  rules rather than granting a bonus (master map, Approach 3; secondhand).
- **Boundary:** asymmetry that only moves numbers is shallow, so a "kind" that never reaches the
  decision loop adds a variant, not a start.
- **Feeds:** Approach 3 (asymmetric starts/factions).
- **For the mod:** patron-Sephirah factions should each rewrite rules and available abnormalities,
  and fewer, deeper patrons beat full Sephirot coverage.

### Randomize the pairings, or the system gets re-solved one level up *(provisional)*

Players don't memorize pieces, they memorize solutions to pairings, so fixed pairings collapse into
a lookup table one abstraction level above the pieces; shuffling which pieces meet keeps the learned
knowledge general instead of rote.

- **Evidence:** Master map, Design principles — stated as rubric; no listed reference argues it
  directly yet. Monster Train — pairing two clans per run multiplies starts without authoring each
  combination (master map, Approach 3; secondhand, and the pairing link is inferred rather than
  stated).
- **Boundary:** it breaks when one pairing is strictly dominant, because the player then rerolls
  toward it mentally, so the pool needs rough pairing parity.
- **Feeds:** Approaches 1 (drafted loadout), 2 (procedural/affixed content).
- **For the mod:** shuffled work-affinities and abnormality-agent pairings, so a memorized work
  matrix can't be carried between runs.

## Fairness & legibility

What keeps imposed variance fair, because a player who stops trusting the rolls stops replaying.

### Fair means legible: telegraph the stakes even when you hide the specifics *(established)*

Players accept losing to a risk they could read, because the loss converts into knowledge for the
next attempt; hidden stakes convert the same loss into resentment. A short modifier vocabulary is
part of the telegraphing, because an alphabet you can hold in your head reads at a glance.

- **Evidence:** Into the Breach — perfect information with varied objectives, the proof that
  legible and varied aren't in tension (master map, Approach 5; secondhand). Against the Storm —
  glade events couple every procedural reward to a telegraphed hazard (notebook Mechanic inventory,
  2026-07-01; firsthand verification pending). Risk of Rain 2 — a handful of elite kinds recombine
  with any enemy and stay readable, restraint as legibility (master map, Approach 2; secondhand).
- **Boundary:** the Binding of Isaac marks where variance crosses into unfair (uncapped swings with
  no read and no agency), and Returnal's hidden-benefit items show the end state: skilled players
  rationally ignore a layer they can't price.
- **Feeds:** Approaches 1 (drafted loadout), 2 (procedural/affixed content), 5 (structural
  run-restructuring).
- **For the mod:** a rerolled abnormality must state its stakes at containment (risk tier, rough
  category) before the player commits, or blind-pick breaks its own rule.

### Never leave the player zero workable line *(established)*

The roguelite contract is that mastery pays, so a state where no play works, however rare, breaks
the contract retroactively: the player stops trusting that their learning mattered. Spelunky
enforces it at the generator (carve the critical path first, decorate after), which is the
strongest form, an invariant by construction rather than by testing.

- **Evidence:** Spelunky — every level guaranteed solvable, because the generator carves a critical
  path before it decorates around it (master map, Approach 2; secondhand). Blue Prince — the
  counter-example, rolling RNG that blocks applying knowledge you already earned (master map,
  Approach 1; secondhand).
- **Boundary:** the guarantee covers a workable line existing, not a comfortable one, so difficulty
  spikes are fine as long as some line survives.
- **Feeds:** Approaches 2 (procedural/affixed content), 5 (structural run-restructuring).
- **For the mod:** any generator (starting facility conditions, affix stacks, ordeal slates)
  validates a survivable configuration before decorating with pressure, by construction, not by
  playtest luck.

### No imposed run-enders *(provisional)*

A run may end from a gamble you opted into, because you priced the risk when you took it; an
unprovoked corner-kill teaches nothing and reads as the system cheating, which poisons trust in
every other roll.

- **Evidence:** Against the Storm — glade hazards bite only after you accepted the
  reward-behind-risk offer (notebook Mechanic inventory, 2026-07-01; firsthand verification
  pending). Master map, Design principles — stated as rubric with the opt-in distinction.
- **Boundary:** "opt-in" must be real, because a forced pick between punishments is an imposition
  wearing a choice's clothes.
- **Feeds:** Approaches 2 (procedural/affixed content), 6 (emergent systems + adaptive director).
- **For the mod:** an ordeal or meltdown may escalate pressure, but a run-ending spike must trace
  back to a priced player decision (a risky work order, a power dial, a cracked containment),
  never a flat roll.

## Recombination & drafts

Why contextual value is the engine of drafted variance, because a solved piece is a dead piece.

### Make the pieces interact: value must be contextual *(established)*

When a piece's value is fixed, evaluating it once solves it forever and the draft becomes a lookup;
when value depends on what you already hold, the same offer poses a different question every run,
so the variance lives in the interactions rather than the pieces.

- **Evidence:** Slay the Spire — a card that is dead weight in one deck is a build-around in
  another, and relics swing whole archetypes (notebook Mechanic inventory; master map keystone,
  Approach 1; secondhand). Against the Storm — the clan mix tilts which drafted buildings pay off
  (notebook Mechanic inventory, 2026-07-01; firsthand verification pending).
- **Boundary:** it breaks at both densities, because interactions everywhere mean every pick works
  (choice stops mattering) and interactions nowhere mean one right pick. The open question both
  notebooks are chasing sits here too: whether the *offer* must read your state, or a flat pool of
  contextually-valued pieces suffices.
- **Feeds:** Approach 1 (drafted loadout).
- **For the mod:** abnormality value must depend on the roster, facility, and patron you hold, so
  the draft-of-three poses a fresh question each day.

### A small memorizable base stays novel when the modifiers run deep *(established)*

Novelty doesn't need a huge content pool, because modifiers multiply a known base combinatorially;
the base staying memorizable is what keeps the recombination legible, so the split is base equals
knowledge, modifiers equal variance.

- **Evidence:** Balatro — eight poker hands recombined through a 150-plus joker pool (master map,
  Approach 1; secondhand). Diablo — affixes rolled onto known monsters, the reference
  implementation of recognizable-but-modified (master map, Approach 2; secondhand). Hades — known
  enemies arrive armored, shielded, or split (master map, Approach 2; secondhand).
- **Boundary:** Slay the Spire marks the opposite pole, leaning on a 350-plus card pool, which
  means the small-base route has to load more recombination onto each piece to compensate.
- **Feeds:** Approaches 1 (drafted loadout), 2 (procedural/affixed content).
- **For the mod:** LobCorp's roster is an order of magnitude smaller than StS's pool, so this is
  the load-bearing lesson: keep abnormalities memorizable, put the variance in affixes and
  pairings.

### Couple cost to power *(established)*

If the strong option is also the cheap one, every choice self-solves and the variance dies at the
pick; taxing the tempting picks keeps each offer a genuine toss-up, so the interesting decision
survives contact with a spreadsheet.

- **Evidence:** Returnal — parasites fuse a benefit to a drawback, the one system its reviewers
  agree works (master map, Appendix; secondhand). Diablo — affix stacks stay bounded so power
  arrives packaged with threat (master map, Approach 2; secondhand).
- **Boundary:** the cost must be legible at pick time, because Returnal's hidden-benefit items show
  what happens otherwise: risk becomes unreadable and skilled players skip the whole layer.
- **Feeds:** Approaches 1 (drafted loadout), 2 (procedural/affixed content).
- **For the mod:** the strongest abnormalities and E.G.O. carry proportionate containment or
  economy costs, priced visibly at pick time.

## Pacing & escalation

How pressure stays fresh across runs, because a memorized difficulty curve is just more fixed
content.

### Pace threats off live state, not a script *(established)*

A scripted curve gets memorized along with the rest of the game; a director that reads how you're
doing regenerates the pressure curve every run, so the pacing itself becomes a variance source.

- **Evidence:** Left 4 Dead — the AI Director spawns threats and tunes intensity off live player
  state, pacing peaks against lulls (master map, Approach 6; secondhand). RimWorld — the
  Storyteller weighs escalation against colony strength and wealth (master map, Approach 6;
  secondhand). dotAGE — Prophecy events scale late-run threats off run progress (master map,
  Approach 6; secondhand).
- **Boundary:** the signals have to be re-derived per game, because L4D reads health and progress
  in real time while a LobCorp director would read work outcomes and panic over a day; porting
  another game's inputs breaks the read.
- **Feeds:** Approach 6 (emergent systems + adaptive director).
- **For the mod:** make the existing ordeal/meltdown scheduler variable and state-aware rather than
  building a director from nothing, which is the cheap near-term slice the map's roadmap already
  calls out.

## Persistence & memory

What survives a run and why, because retention and cheap narrative both live here.

### Failed runs must fund permanent progress *(established)*

A roguelite asks the player to lose often, so retention depends on every death buying a durable
step forward; when failure converts into currency, the loss stops being wasted time and the loop
stays voluntary.

- **Evidence:** Hades — the Mirror of Night spends earned darkness on persistent upgrades, so every
  death funds a permanent step (master map, Approach 4; secondhand). dotAGE — memory points, a
  roguelite research tree inside a management game (master map, Approach 4; secondhand). Rogue
  Legacy — the lineage system, each heir starts stronger (master map, Approach 4; secondhand).
- **Boundary:** it breaks when meta-power trivializes the run game, so progress has to widen
  approach rather than just add stats (the next lesson).
- **Feeds:** Approach 4 (meta-progression/unlock divergence).
- **For the mod:** the research tree reimagined as a roguelite unlock web is the retention glue,
  because a wiped facility still banks research.

### Unlocks should widen what can appear, not front-load power *(established)*

Progression that only adds starting power makes later runs easier but not different, which spends
replayability to buy retention; unlocks that expand the possibility pool make later runs more
varied, so the two currencies stop competing.

- **Evidence:** Dead Cells — blueprints feed the run's drop tables, widening what can appear rather
  than what you start with (master map, Approach 4; secondhand). Hades — Mirror upgrades change how
  you approach a run, not just raw power (master map, Approach 4; secondhand).
- **Boundary:** pure pool-widening with no power floor frustrates, and where to split power between
  within-run and across-run is dotAGE's hardest balance question, still open.
- **Feeds:** Approach 4 (meta-progression/unlock divergence).
- **For the mod:** unlocks admit new abnormalities, E.G.O., and mechanics into the roll pools
  rather than starting the facility stronger.

### Back-fill causality around real events and let apophenia finish the story *(established)*

Generated narrative is cheap when it decorates events that actually happened mechanically, because
the player's pattern-hunger supplies the connective tissue; simulating the story first is the
expensive trap.

- **Evidence:** Caves of Qud — ex-post-facto history generation, artifacts first and causality
  back-filled (master map, Approach 6; secondhand). Darkest Dungeon — the Graveyard, deaths
  becoming persistent fixtures, the anchor of the Memorial worked example (master map, worked
  example; secondhand).
- **Boundary:** it breaks when the generated text contradicts what the player watched happen,
  because the mechanical event is the ground truth the text has to wrap.
- **Feeds:** Approaches 6 (emergent systems + procedural history), 4 (meta-progression).
- **For the mod:** the Memorial system is this lesson shipped: incident reports, plaques, and
  trauma journals templated around real deaths, in LobCorp's native corporate-clinical voice.

## Decision log

- **rev 1, 2026-07-05.** Seeded the doc at install time from three sources: the master map's seven
  Design principles (argued in full here, one-liners kept there per the boundary rule), the
  principles previously trapped inside the map's approach prose (small-base-deep-modifiers, the
  Spelunky solvability guarantee folded into zero-workable-line, failed-runs-fund-progress,
  widen-the-pool, pace-off-state, and cheap generated history), and the two notebooks' graduating
  sections as corroborating evidence. Thirteen entries across five themes. Seed evidence is mostly
  secondhand (the map's reads), marked honestly per entry, and the Slay the Spire flat-pool
  hypothesis stays out until its assigned task lands, because unverified hypotheses are not
  lessons.

## Syncing

This document lives in two places: `docs/roguelite-replayability-lessons.md` in the repo (the
canonical body) and a twin in the Obsidian vault (the same body, plus YAML frontmatter). Edit
either one, then run `/sync-design-doc` (pair `roguelite-lessons`) to reconcile, because it diffs
the two bodies, propagates the newer one, and stages the vault side for review. Keep substantive
design changes here, so the repo history captures them.
