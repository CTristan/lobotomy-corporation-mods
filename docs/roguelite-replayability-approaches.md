# Lobotomy Corporation — Replayability Approaches & Research Map

> **What this is.** A research map and decision aid for the **Roguelite mode** mod (epic **#79**). It
> ranks games by how directly their replayability lessons lift into LobCorp, then ties those lessons
> to the live GitHub issues. It is not committed scope or a spec. The issues (#79 and its children
> #150–166) are the source of truth for what ships, so treat this doc as where the thinking and the
> decisions live, not as a promise of work.
>
> **Living document.** The research half (the problem through the appendix) is stable. The
> project-tracking half (the approach-to-issue map, the open questions, the decision log) changes as
> the mod evolves. A twin copy lives in the Obsidian vault, so read **Syncing** at the end before you
> edit either one.

**Ranking criterion:** games are judged by their relevance and applicability to building a LobCorp roguelite mod, which means how directly their lessons can be researched and lifted, not how deep the idea is in the abstract. Each game gets one **game-level calibration score**, an anchored ordinal on a 0–100 scale, not a probability and not a percentage, which is its overall applicability across every lever it teaches. Because a game almost always teaches one lever better than the rest, each approach also places that game by a **per-lever score** on the same scale, and the per-lever score is what orders the references inside an approach. The two meet at the game's strongest lever, so the game-level number is the ceiling its best approach reaches and a secondary lever scores at or below it, which is why dotAGE reads as 86 at its meta-progression peak in Approach 4 but 85 at Approaches 2, 3, and 6. The bands carry the meaning, and the number inside a band is a soft ordering, not a precise distance, so 86 over 85 means "same tier, roughly this order," not "one percent better."

In the reference lists this shows up as one number or two. A bullet reads `**Game (NN).**` when the approach is that game's strongest lever, because the per-lever score equals the game-level score there. It reads `**Game (NN overall, MM here).**` when the approach is a secondary lever, where `NN` is the game-level score and `MM` is the lower per-lever score that sets the ordering. Two numbers is the signal that the game teaches this lever well but teaches another one better, so read `MM` as its rank here and `NN` as its overall standing.

The bands, named with representative references rather than a full roster:

- **95–100, reserved.** No game has earned it yet, which keeps room to re-rank if a clearer reference ever turns up.
- **90–94, keystone.** The clearest, most directly liftable references: Slay the Spire (92) and Into the Breach (90).
- **85–89, strong.** Directly liftable: Hades (88), FTL (88), Left 4 Dead (88), Darkest Dungeon (87), dotAGE (86), RimWorld (86), Diablo affixes (85), and Risk of Rain (85).
- **80–84, solid.** Narrower or partly redundant: Balatro (84), Blue Prince (83), Spelunky (82), Monster Train (80), and Caves of Qud (80).
- **Below 80, cut.** No section slot, so it goes to the Appendix with a one-line reason.

A section lists every reference that clears the 80 per-lever floor, in descending per-lever order, with no fixed count, so the score is the only gate on entry. What keeps a section from sprawling is redundancy, not a cap, because a game that clears 80 but only re-teaches a lever-lesson a listed reference already covers more directly is an Appendix cut, not a slot. That is why a lever many games teach well, like procedural content, carries more references than a narrow one, and the difference is information rather than noise.

The scale is provisional, not fixed. If an evaluated game breaks the bands, by earning the reserved 95–100 or by fitting none of them, that forces a re-anchor. Log it under **Changes under the applicability criterion** in the Notes and in the Decision log. A cautionary game scores the same way, on whether its warning is *uniquely* instructive: the Binding of Isaac scores high (85) because it is the canonical swinginess anti-pattern, a warning nothing else in the list delivers as cleanly, while Returnal scores low because its warning duplicates lessons already taught. **Confidence** is second-order: my certainty that the read is correct, which can stay high even on a negative verdict, because a clean "this teaches nothing new" is a high-confidence cut.

## The problem

A LobCorp campaign converges. The only between-day randomness is the draft-of-three abnormality offering, and it doesn't generate replay value, because:

- It's a **choice**, so you steer rather than adapt to imposed variance.
- Abnormalities are **solved in isolation**, so a different draw never demands a different decision.
- You draft most of the pool over a campaign anyway, so every run lands on the same end-state.

Replayability needs variance that is **imposed**, **forces deviation** from a known-optimal line, and is **recombinant** (it stays novel after you've learned every piece).

## Design principles

These cut across every approach below, because they're how you keep any of them fair and legible.
Each one is argued in full, with its evidence and boundaries, in
`docs/roguelite-replayability-lessons.md`, so this list stays the one-line rubric and that doc
holds the reasoning.

- **Forced, not chosen, variance.** The system hands you a different problem each run.
- **Make the pieces interact.** Value is contextual, not solved in a vacuum.
- **Fair means legible, not hidden.** Telegraph the stakes even when you hide the specifics (the Into the Breach rule).
- **No imposed run-enders.** A gamble you opted into may bite, but the game may not corner you unprovoked.
- **No unrecoverable corner.** Never leave the player zero workable line (the Blue Prince lesson).
- **Couple cost to power.** Make every option a genuine toss-up by taxing the tempting picks, not the weak ones.
- **Randomize the pairings.** Otherwise the system gets re-solved one level up.

## The six approaches

### 1. Drafted loadout + interacting pieces
- **Lever:** variance from recombining a known pool, with synergies and conflicts between the pieces you hold.
- **Research:**
  - **Slay the Spire (92).** It's the keystone for this lever because card value is contextual, so a card that's dead weight in one deck is a build-around in another, which means the variance lives in how the pieces interact rather than in the pieces themselves. Study how draft offers are conditioned on what you already hold and how relics swing the value of whole archetypes, because that conditioning is what the blind-pick POC has to reproduce. The scale won't transfer, because StS leans on a 350-plus card pool to stay fresh and LobCorp's abnormality roster is an order of magnitude smaller, so each piece has to carry more recombination.
  - **Against the Storm (86).** It's the management-genre proof for this lever, because each settlement drafts its production buildings and cornerstone perks from randomized offers at fixed reputation milestones, so your available systems are a drafted loadout that recombines run to run rather than a fixed build list. Study how the blueprint and cornerstone offers condition on the three clans and the resources you already hold, because that in-genre conditioning is the closest working model for the abnormality draft the POC has to build. The real-time settlement sim won't transfer, because Against the Storm recombines buildings across a live economy and LobCorp resolves work day by day, so the draft-and-interact lesson lifts while the moment-to-moment pacing doesn't.
  - **Balatro (84).** It earns its tier here by getting emergent synergy from a tiny fixed set, the eight poker hands, recombined through a 150-plus joker pool, which proves a small memorizable base stays novel when the modifiers run deep. Study the joker-to-hand interaction, because the lesson is that the modifiers carry the variance while the base set stays known, which maps straight onto abnormalities-plus-affixes. The pure-score loop won't transfer, because Balatro only asks you to beat a rising chip threshold and LobCorp runs need objectives with texture instead.
  - **Blue Prince (83).** It supplies the day-draft structure, the run assembled one drafted room at a time, so it's the closest structural model for a between-day abnormality draft. Study how it gates and sequences that draft across a day, because the pacing is directly liftable. Its failure mode is the caution, because Blue Prince keeps rolling RNG that blocks you from applying knowledge you already earned, which breaks the no-unrecoverable-corner principle, so study it as much for what to avoid as for what to copy.
  - **Monster Train (80).** It layers synergy vertically, stacking clan mechanics across three battlefield floors, which extends the drafted-loadout idea past a flat hand. Study the cross-floor combo-building, because it shows how to fold positioning into the recombination. The dual-clan board won't transfer, because it's tuned to Monster Train's tower-defense lanes and LobCorp has no equivalent spatial layer, so only the synergy-depth lesson carries.
- **LobCorp:** the conditioned-draft, blind-pick, lockdown work, which is the POC. Blind-pick hides *which* abnormality you drew, but it has to keep the stakes legible (the risk tier, the rough category), or it breaks the Into the Breach rule.

### 2. Procedural / affixed content
- **Lever:** the content itself varies each run, either generated outright or recognizable-but-modified through rolled affixes.
- **Research:**
  - **Hades enemy variants (88 overall, 86 here).** It scores high here because it layers affixes onto known enemies, so a familiar foe arrives armored, shielded, or split, which keeps recognizable content from going stale. Study how the variant rolls scale with heat and region, because the lesson is affixes that ramp with difficulty rather than rolling flat. The action read won't transfer, because Hades telegraphs variants through animation and LobCorp resolves work through menu-driven stats, so the affixes have to be legible at containment instead.
  - **Diablo-style monster affixes (85).** It's the canonical affix system, the template every later roll-traits-onto-enemies design descends from, so it earns its place as the reference implementation. Study the affix-stacking rules and how combinations stay bounded, because incoherent or unfair stacks are the first failure to design out. The loot purpose won't transfer, because Diablo affixes exist to justify gear churn and LobCorp's exist to break a memorized work-matrix, so the mechanism copies cleanly even as the goal reframes.
  - **dotAGE (86 overall, 85 here).** It's the strongest in-genre proof here because it puts procedural variance inside a *management* game, a randomized building pool plus a reshuffled tech tree each run, which is the closest structural match to LobCorp in the list. Study how it keeps a management economy fair while the available pieces shift run to run, because that's the exact tension affixed abnormalities create. Little of it fails to transfer, because the genre and the solo-build scope already line up, so this is the reference to study most closely for this lever.
  - **Against the Storm (86 overall, 85 here).** It earns a place here as a second in-genre proof next to dotAGE, because every settlement rolls a biome, a starting-goods hand, a blueprint pool, and a set of glade events, so the content you manage is regenerated each run rather than re-skinned. Study the glade-event system specifically, because it couples every procedural reward to a telegraphed risk, which is the legible-affix bar a rerolled abnormality has to clear. The forest-biome framing won't transfer, because Against the Storm rolls terrain, wildlife, and hazards and LobCorp has no generated map, so the roll-the-content principle lifts without the spatial layer.
  - **Risk of Rain 2 elite types (85).** It shows affixes as a small readable vocabulary, the handful of elite modifiers (blazing, overloading, and the rest) that recombine with any enemy, which keeps the system legible. Study how few modifier *kinds* it needs to stay varied, because the lesson is restraint, a short affix alphabet beats a sprawling one. The timer pressure won't transfer, because RoR2 ramps elites off a run clock and LobCorp paces by day, so the trigger changes even though the affix design holds.
  - **The Binding of Isaac (85).** It scores as the canonical counter-example, the swinginess to avoid, because its item rolls can hand one run a god-tier synergy and the next a dud with no agency in between. Study exactly where its variance crosses from exciting to unfair, because naming that line is what keeps an affix system from repeating the mistake. None of its uncapped-variance model should transfer, so study it as the boundary the rolls must stay inside, not as a design to lift.
  - **Spelunky (82).** It's the model for *fair* procedural generation, because every level is guaranteed solvable, since the generator carves a critical path before it decorates around it. Study that solvability guarantee, because it's the procedural-fairness invariant LobCorp's rolls need. The spatial frame won't transfer, because Spelunky's generation is level geometry and LobCorp has no generated map, so only the fairness principle lifts, not the technique.
- **LobCorp:** abnormalities roll traits and affixes per run ("enraged," "symbiotic," shuffled work affinities) so the memorized work-matrix breaks. Keep the rolls legible at containment, because this is exactly where fairness gets tested.

### 3. Asymmetric starts / factions
- **Lever:** runs differ because you begin as a fundamentally different "kind."
- **Research:**
  - **Slay the Spire characters (92 overall, 90 here).** It scores highest here because each character is a different *kind* of run, a distinct starting deck, relic, and mechanic that reshapes strategy from turn one, not a stat swap. Study how few characters it needs, just four, to feel fully distinct, because the lesson is depth-per-faction over faction-count. The closed-deck purity won't transfer cleanly, because StS characters never mix and a patron-Sephirah system may want partial overlap, so treat the hard-asymmetry model as a starting point, not a constraint.
  - **Hades weapons and aspects (88 overall, 87 here).** Its asymmetry comes from the infernal arms and their aspects, where each weapon plays differently and aspects re-skin one weapon into fresh playstyles, so it's asymmetry layered on a shared base. Study the aspect system specifically, because it shows how to get many distinct starts from a few base kinds cheaply. The manual-aim identity won't transfer, because a weapon's feel in Hades lives in the dodging and aiming and LobCorp expresses identity through rules and available abnormalities instead.
  - **Darkest Dungeon party comps (87).** Its variance is the party you assemble, because hero classes combine into comps with sharply different capabilities, so the "kind" you start as is emergent from the team. Study how class synergies and conflicts create comp identity, because that's how a few units yield many distinct starts. The four-slot party frame won't transfer directly, because LobCorp's agents aren't a fixed-size combat party, so the comp lesson informs faction design without mapping unit-for-unit.
  - **dotAGE Elders (86 overall, 85 here).** It does asymmetric starts through Elders, leaders that each change a run's mechanics, which is a direct genre-proof of the patron-Sephirah idea inside a management roguelite. Study how an Elder reshapes the whole run's rules rather than just granting a bonus, because that's the bar a patron-Sephirah should clear. Little fails to transfer, because the genre and scope already match, so it's the load-bearing reference for this lever.
  - **Risk of Rain survivors (85).** Each survivor is a different kit with its own mobility, damage, and risk profile, so the run reshapes around who you start as. Study how survivor kits change the moment-to-moment decision loop, because asymmetry that only moves numbers is shallow. The shooter expression won't transfer, because a survivor's identity lives in real-time abilities and LobCorp has no such kit, so the principle carries but the form doesn't.
  - **Monster Train clans (80).** Its clans give asymmetric starts by pairing two factions per run, each with a distinct mechanical theme, so the combination is the identity. Study the clan-pairing system, because mixing two asymmetric sets multiplies starts without authoring each one. The lane-combat dependency won't transfer, because clan mechanics are tuned to the tower's floors, so only the pairing concept lifts.
- **LobCorp:** patron-Sephirah factions, where you pick an allegiance that reshapes the whole run (available abnormalities, modified rules, win conditions). We're already building this in the faction-allegiance mod, so it's a replayability engine we've half-built.

### 4. Meta-progression / unlock divergence
- **Lever:** runs diverge because what's *available* expands across many runs, and persistent unlocks reshape play.
- **Research:**
  - **Hades' Mirror of Night (88).** It's the model for meta-progression that reshapes play, because the Mirror spends earned darkness on persistent upgrades that change how you approach a run, not just raw power. Study how it keeps failed runs productive, because every death funds a permanent step forward, which is the retention engine a LobCorp research tree needs. The narrative-gated pacing won't transfer, because Hades ties progression to a story that resolves and a roguelite LobCorp loop is open-ended, so the structure copies but the authored arc doesn't.
  - **dotAGE (86).** It's the closest working model in the list, because its memory-points and "Elder remembers more" system is literally a roguelite research tree in a management game, near-exactly what's proposed. Study how memory persistence splits power between within-run and across-run, because that split is the hardest balance question here. Almost all of it transfers, because the genre and the unlock-web shape already match, so study it as the reference build.
  - **Dead Cells' unlock pool (85).** Its meta-progression is a pool of blueprints fed into the run's drop tables, so what you've unlocked widens what can *appear*, not just what you start with. Study how unlocks expand the option space rather than front-loading power, because that keeps later runs varied instead of merely stronger. The twitch-weapon feel won't transfer, because Dead Cells' unlocks are weapons judged by hit-feel and LobCorp's are systems and abnormalities, so the pool mechanic lifts but the content type changes.
  - **Rogue Legacy (85).** It pioneered persistent between-run growth, the lineage and castle-upgrade system that makes each heir start stronger, so progression is the spine of the loop. Study the trait-and-upgrade cadence, because it shows how steady permanent gains sustain a long series of runs. The action-platformer base won't transfer, because the growth is tuned to twitch difficulty and LobCorp's difficulty is managerial, so the cadence informs pacing but not the numbers.
- **LobCorp:** the research tree reimagined as a roguelite unlock web, with abnormalities, E.G.O., and mechanics opening up *across* runs rather than within one. Its job is retention, so that failed runs feel productive, which makes it the glue for the variance levers rather than a standalone.

### 5. Structural run-restructuring
- **Lever:** change the *shape* of a run (objectives, starting conditions, branching), not just its contents.
- **Research:**
  - **Into the Breach (90).** It's the keystone for restructuring because it varies islands, squads, and objectives, so the *shape* of a run changes rather than just its contents, and it does this while keeping every threat fully telegraphed. Study how it pairs perfect information with varied objectives, because it's the proof that legible and varied aren't in tension (the Into the Breach rule the whole doc leans on). The eight-tile board won't transfer, because ITB's structure lives in a tiny perfect-information grid and a LobCorp run unfolds across a facility over a day, so the legibility principle lifts but the spatial form doesn't.
  - **FTL (88).** It restructures runs through sectors, ship starts, and branching events, so a run's path and pressures differ each time rather than just its loot. Study the branching sector map, because it's a clean model for giving a run varied objectives and routes cheaply. The constant-flight attrition won't transfer, because FTL's tension is a resource race against a pursuing fleet and LobCorp's day structure isn't a chase, so the branching lesson carries but the pressure model doesn't.
  - **RimWorld scenarios (86 overall, 85 here).** Scenarios reshape the starting conditions, the crashed-ship, tribal, or naked-brutality openings that change what a run even is from the first minute. Study how a scenario rewrites starting rules and constraints, because that's the direct analog of randomized starting facility conditions. The open-sandbox endpoint won't transfer, because RimWorld scenarios feed an unbounded colony sim and a LobCorp run needs a defined arc, so the start-condition lever lifts without the open-endedness.
- **LobCorp:** varied run objectives and randomized starting facility conditions, so runs are shaped differently rather than just stocked differently. High ceiling, heavy build.

### 6. Emergent systems + an adaptive director
- **Lever:** replayability from systems that produce unpredictable situations, paced by a director that reads game state.
- **Research:**
  - **Left 4 Dead's AI Director (88).** It's the most directly liftable director model, because the AI Director spawns threats and tunes intensity off live player state, the canonical "drama manager" that reads how you're doing and responds. Study how it measures player stress and paces peaks against lulls, because that feedback loop is exactly what a meltdown/ordeal director would run. The shooter readout won't transfer, because L4D infers stress from health and progress in real time and LobCorp would read it from work outcomes and panic over a day, so the loop copies but the signals change.
  - **RimWorld's Storyteller (86).** It's the buildable face of emergent simulation, a director (Cassandra, Phoebe, Randy) that paces threats against the colony's state and wealth, so the challenge curve is authored by a system, not a script. Study how the Storyteller weighs escalation against the player's current strength, because pacing-off-state is the heart of this lever. The wealth-triggered raid math won't transfer, because it's tuned to a colony economy and LobCorp's pressure would key off containment and panic, so the director concept lifts but its inputs are re-derived.
  - **dotAGE Prophecy events (86 overall, 85 here).** It offers a clean threat-pacing model through escalating Prophecy events, because each run ramps to threats far harder than the early ones, a director-shaped curve inside a management roguelite. Study how it scales late-run threats off run progress, because it's the in-genre proof that a managed escalation curve works. Little fails to transfer, because the genre and turn-based pacing already match, so it's the closest reference for a LobCorp-shaped escalation.
  - **Caves of Qud (80).** It supplies procedural *narrative* texture cheaply, through ex-post-facto history generation, so it generates the artifacts, back-fills the causality, and lets player apophenia finish the story. Study the history-generation technique specifically, because it's the cheap, liftable version of deep emergence and the engine behind the Memorial worked example. The full simulation depth won't transfer, because Qud sits on a sprawling systemic world LobCorp won't simulate, so take the history-texture trick and leave the simulation behind.
  - **Dwarf Fortress (foundational reference, not a competitive entry).** It's the ancestor of this whole approach, full-simulation emergence, so it sets the ceiling the lever reaches for. But it's effectively unbuildable in a focused mod, and its usable lessons are already captured above by RimWorld (the director) and Qud (cheap procedural history), so study it for inspiration and reach its value through those refinements. Treating it as a build target is the trap, which is why it lives here as a foundation and in the Appendix as a real cut.
- **LobCorp:** a director that schedules meltdowns, ordeals, and events dynamically per run, plus procedurally-generated facility history and lore (see the worked example below).

## Worked example — the Memorial / facility-history system

A concrete feature that fell out of the procedural-narrative-texture idea, anchored on Darkest Dungeon's Graveyard and extended through Qud's history technique. It's worth calling out because one cheap system spans three approaches at once: sticky consequences (DD), procedural narrative texture (Qud), and meta-progression (cross-run persistence).

- **Richer incident artifacts.** A death generates a textured after-action report in LobCorp's corporate-clinical voice (a redacted memo, the agent's final log entry, a colleague's note), back-filled around the mechanical event you already know.
- **Cross-run persistence.** Past runs' catastrophes become fixtures in later runs: a marked sealed-off cell, a memorial plaque, a veteran whose file notes they "survived the [generated incident]," an abnormality log referencing a prior failure. Your history writes the lore your next run is set in.
- **Survivor trauma as story.** Breach-survivors carry narrative quirks tied to a specific recorded event, not just stat-debuffs.
- **Relics from the fallen.** A dead agent's E.G.O. gear, or an abnormality's record, bears the generated mark of who fell to it.
- **Why it's cheap.** Every layer is generated text templated around events that already happened mechanically, so there's no simulation and the player's apophenia does the heavy lifting. It's on-brand too, because LobCorp's native voice *is* incident reports and logs.

## Prioritization roadmap

Ranked on impact (does it move the core needle?), effort (cost to build solo), and fit (including leverage off work already in progress).

1. **Build first: the conditioned draft (Approach 1).** Lowest effort, high fit, real impact. It gets a replayable loop running and proves it's fun before anything heavier.
2. **High-leverage slot: asymmetric factions (Approach 3).** Recommended, because we're already building the patron-Sephirah mod, so it's high impact at low *marginal* cost and serves two projects at once. The alternative is procedural/affixed content (Approach 2), which has the highest raw impact on the convergence problem but is medium-high effort and touches core behavior, so slot it right after factions as the deep-variance play.
3. **The glue: meta-progression (Approach 4).** Moderate effort, high fit, and it drives retention. Best added once you have a variance lever to progress toward.
4. **Ceiling and scope-trap: structural restructuring (5) and deep emergent simulation (6).** Highest ceilings, highest effort, lowest fit. The full-simulation route especially tends to balloon unbounded, so file it under "if the mod takes off," not near-term. That said, the *cheap* slices of Approach 6 (the director and procedural history) are near-term-feasible. It's only the deep-simulation end that's the trap.

## Notes

- The strongest mods combine approaches, and the Memorial system is the proof: one cheap feature doing three jobs.
- Games recur across approaches (Hades teaches both factions and meta-progression, for instance), which is expected, so study each through the lens of the approach you're building.
- **dotAGE is the closest holistic analog** to the whole project, a solo-built, turn-based management *roguelite*, which is why it legitimately shows up across Approaches 2, 3, 4, and 6 rather than in one. Study it as a complete reference for the overall shape, not just per-lever.
- **Against the Storm is the second in-genre anchor**, a commercially proven management roguelite that pairs with dotAGE across the draft and procedural levers (Approaches 1 and 2). Where dotAGE maps closest on shape, because it's turn-based and solo-built like the mod, Against the Storm is the cleanest proof that drafting your buildings and perks each run keeps a builder replayable, so read them together, one for the discrete structure and one for the draft-and-reroll engine.
- **Changes under the applicability criterion:** Dwarf Fortress moved from a competitive entry to a foundational reference (a great idea, but unbuildable, so its value lives in RimWorld and Qud). Left 4 Dead's AI Director was added as the more applicable director model. Caves of Qud rose to 80 once its lesson resolved into the cheap, liftable procedural-history technique. The score bands were introduced and made revisable, and Into the Breach moved from the strong band to keystone (90) without changing its number. Per-lever scoring was added, so each game now carries a per-approach score alongside its game-level score, and that per-lever score orders the reference lists. It moved Diablo-style affixes above dotAGE and Against the Storm in Approach 2, because affixed content is Diablo's whole identity but a secondary application for the two management games. No game-level score changed. The per-section count cap was then removed, so a reference earns its slot by clearing the 80 per-lever floor without being redundant, and no fixed count limits a section.

## Appendix — evaluated but not selected

These games came up during the research but earned no listed reference slot in any approach, judged against the applicability criterion. A game lands here when it scores below 80 at every lever, or when it clears 80 but a listed reference already teaches its lever-lesson more directly, so it adds nothing new. No per-section count forces a cut, because a reference earns its slot by clearing the 80 per-lever floor without being redundant, so these are the real cuts on merit rather than the overflow from a headcount.

- **Dwarf Fortress** is the ancestor of the emergent-systems approach, but full-simulation emergence is effectively unbuildable in a focused mod, and its usable lessons are already captured by RimWorld (the director) and Qud (cheap procedural history). It stays in Approach 6 as a foundational reference rather than getting cut outright. Great idea, wrong cost.
- **Library of Ruina** is Project Moon's own deterministic deckbuilder, ranked high early on lineage value. But lineage isn't mechanical applicability, and its actual lesson (deep card interaction in a deterministic system) is taught more directly by Slay the Spire and Balatro, both atop Approach 1. Valuable to you, redundant mechanically.
- **Loop Hero** has one genuinely novel idea (you assemble the gauntlet that then challenges you), but it's a single idea, not a deep system. It's outclassed in drafted-loadout (Slay the Spire, Balatro, Monster Train, and Blue Prince are richer) and in structural-restructuring (FTL, Into the Breach, and RimWorld are more applicable), so it tops no section. Worth one look for the inversion concept, not a primary reference.
- **Inscryption** surfaced in the research and is a superb game, but its strength is a one-time meta-narrative experience, and its roguelite replay mode is secondary. As a *replayability* teacher specifically, the only thing being ranked here, it tops nothing. Right game, wrong question.
- **Returnal** surfaced as a procedural roguelite, but its lessons are either redundant or cautionary. Its fair-procedural-generation trick (hand-built rooms shuffled into fresh orders) is already taught by Spelunky and Into the Breach, and the one system reviewers agree works, the parasites, is just "couple cost to power" made legible, which the Diablo-style affixes already cover. Where Returnal is distinctive it's a warning, because the hidden-benefit items and a meta-progression lighter than Hades' make risk illegible in a high-skill game, so skilled players rationally ignore the roguelite layer. The cautionary tale, not the model.

Also surfaced but not individually evaluated: the searches turned up several recent titles (Shogun Showdown, Sol Cesto, and others) that I didn't drill into, because none clearly served one of the six approaches better than the entries already listed. Worth a glance for fresh examples, but none looked like a missing piece.

---

## Approach → tracked issues

> The research above is the stable reference. Everything from here down is the living project layer,
> mapping the approaches to the GitHub issues and recording decisions as the mod evolves.

The Roguelite mode is tracked as epic **#79** ("Replace Challenge Mode with a repeatable roguelite experience"), which blocks 16 child enhancement issues **#150–166** (each titled `[Roguelite] …`), plus a related infrastructure bug **#164**. Every child is a Darkest-Dungeon-inspired enhancement layered on the base mode, so several were authored as consequence, economy, or flavor systems rather than pure variance generators, and a few resist a clean single-approach fit (marked `*`).

| Issue | What it adds | Approach | Memorial |
|---|---|---|---|
| **#79** | Replace Challenge Mode with a repeatable roguelite mode *(epic / container)* | 5 (+1 core) | — |
| #150 | Cross-run death graveyard with auto-generated epitaphs | 6 | core |
| #151 | Random panic type at each SP break | 2 `*` | — |
| #152 | Survivable Death's Door rolls instead of instant permadeath | 5 | — |
| #153 | Persistent trauma, acquired quirks, LOB treatment | 4 | adjacent |
| #154 | Pair-affinity relationships between agents | 6 | — |
| #155 | Choose the day's ordeal slate for a bounty | 5 | — |
| #156 | Escalating dread meter that prices rewinds | 6 | adjacent |
| #157 | Between-day narrative events from XML packs | 6 | — |
| #158 | Narrator bark engine from community text packs | 6 | — |
| #159 | Day-end performance recap screen | 6 | core |
| #160 | Persistent per-agent trauma journal | 6 | core |
| #161 | Pre-rolled recruit pool with strengths and flaws | 1 `*` | — |
| #162 | Veterancy ranks on a new agent-history layer | 4 | — |
| #163 | Day-start LOB consumables shop | 1 `*` | — |
| **#164** | *(bug)* `AgentHistory.workDay` save-key fix | enabler | enabler |
| #165 | Risk-reward facility power dial | 5 | — |
| #166 | Per-department crests spent on upgrades | 4 | — |

`*` Loose or ambiguous fit, so see Coverage & gaps. `(+1 core)` on **#79** marks that the epic carries the Approach 1 conditioned-draft POC as its base mode, on top of being the Approach 5 container. Approach legend: **1** drafted loadout, **2** procedural/affixed content, **3** asymmetric starts/factions, **4** meta-progression, **5** structural run-restructuring, **6** emergent systems + director + facility history.

### Coverage & gaps

The 16 children cluster on approaches **4, 5, and 6** (the meta-progression, run-restructuring, and emergent-history/director levers). Three approaches are un-ticketed, and they're exactly the ones the Prioritization roadmap above ranks *first*:

- **Approach 1 (the conditioned-draft POC) is un-ticketed at the child level.** The draft, blind-pick, and lockdown core lives in **#79** itself as the base mode, not as a child issue. #161 (recruit draft) and #163 (day-start prep) are loadout-shaped but roster- and economy-scoped, not the abnormality draft.
- **Approach 2 (procedural and affixed *abnormality* content) is un-ticketed.** No issue rolls abnormality traits or affixes, because #151 (panic-type roll) and #161 (recruit-stat roll) randomize other axes.
- **Approach 3 (asymmetric starts and factions) is un-ticketed by design.** This is the patron-Sephirah faction-allegiance mod we're building separately, and #166 is explicitly an *upgrade* system, not factions. See the open questions below.

The adaptive-director sub-idea of Approach 6, a system that reads game state and paces threats off it (the Left 4 Dead and RimWorld definition this doc uses), is effectively un-ticketed. #156 prices rewinds on a static cost ramp, not off live game state, so it doesn't cover the director even though Approach 6 otherwise carries the most issues through its facility-history and emergent-systems clauses. The lift is smaller than it looks, because vanilla LobCorp already ships a non-adaptive ordeal and meltdown scheduler, so the work is making that existing scheduler variable and state-aware rather than building a director from nothing, which is exactly the cheap near-term slice the roadmap calls out.

### The Memorial cluster

The worked example above is partly ticketed. Its core members are **#150** (graveyard and relics of the fallen), **#159** (after-action incident reports), and **#160** (survivor trauma-as-story journal). Adjacent, by your call, are **#153** (survivor trauma, and it shares #150's persistence helper) and **#156** (cross-run-catastrophe theme, though mechanically a difficulty meter). All of them lean on the `AgentHistory` layer that **#164** repairs, so #164 is the cluster's enabler.

## Open questions

- **Approach 3 vs the faction mod.** The source frames factions as already being built in the faction-allegiance mod. Is Approach 3 delivered there and merely referenced here, or does the Roguelite mode get its own faction layer? Settle this before ticketing it.
- **Does the POC need its own issue?** Approach 1 is "build first," but it currently lives inside the #79 epic with no child issue. Decide whether the conditioned-draft POC gets a tracking issue or ships as #79's core deliverable.
- **Build order vs the ticket weighting.** The roadmap ranks 1, then 3, then 4, then 5 and 6, but the existing children are 4/5/6 and explicitly post-release. Do we ticket the gap approaches (1, 2, 3) before building the enhancement layer, or let #79's base mode cover Approach 1 and pick enhancements opportunistically?
- **Memorial as one feature or many?** The doc argues one cheap system spans three approaches. Ship #150/#159/#160 (and maybe #153/#156) as a coherent Memorial feature, or keep them as separate issues?
- **Sequence #164 first.** The `AgentHistory` fix gates the record and journal members (#150/#159/#160) and #162's recording layer, so land it before that cluster.

## Decision log

- **rev 1, 2026-06-27.** Imported the replayability research map as the living design doc for the Roguelite mode (#79). Built the approach-to-issue map from the #79 and #150–166 bodies. Surfaced that approaches 1, 2, and 3 are un-ticketed while the 16 children cluster on 4/5/6.
- **rev 2, 2026-06-27.** Drilled Returnal off the surfaced-games backlog. It scored below the section floor (high confidence), so it lands in the Appendix as a redundant-or-cautionary cut, because its procedural-fairness and cost-coupled-affix lessons duplicate Spelunky, Into the Breach, and the Diablo affixes, and its one distinctive trait (illegible risk plus thin meta-progression in a high-skill game) is a warning the doc already teaches. Removed it from the surfaced-but-not-evaluated line.
- **rev 4, 2026-06-27.** Restructured the research half so each approach's references read as a bulleted list in descending score order, with three approach-focused sentences per game: why it earns its tier through that lever, what to study in it, and what about it wouldn't transfer to the mod. The game-level score is unchanged and identical wherever a game recurs, so only the rationale prose is lens-specific, which is why a multi-approach game like dotAGE now carries different notes under Approaches 2, 3, 4, and 6. No scores moved and the Appendix cuts keep their one-line verdicts. Updated the eval skill to write this format for new games.
- **rev 3, 2026-06-27.** Adversarial methodology pass over the whole map. Reframed the scores as one game-level calibration score per game on a banded 0–100 scale (95–100 reserved, 90–94 keystone, 85–89 strong, 80–84 solid, below 80 cut), with the within-band number as a soft ordering and the scale itself revisable when a game breaks the bands. Dropped the percent glyph, because the scores are anchored ordinals, not percentages. Into the Breach moved from the strong band into keystone (90), the only reclass, and no number changed. Made the cautionary-scoring rule explicit (a warning scores on whether it is *uniquely* instructive). Fixed internal inconsistencies: #156 is a static rewind ramp and doesn't cover the adaptive director, #79 carries the Approach 1 POC as well as the Approach 5 container, and the blind-pick POC now states it must keep stakes legible per the Into the Breach rule. Removed leftover background lore and restatement fluff.
- **rev 5, 2026-07-01.** Added per-lever scoring and drilled Against the Storm. Per-lever scoring: each game keeps its game-level calibration score and now also carries a per-approach score, shown as a second number on a bullet when the approach is a secondary lever, and that per-lever score orders each Research list. It reordered only Approach 2, moving Diablo-style affixes above dotAGE and Against the Storm, and changed no game-level score. Against the Storm scored 86 (high confidence), tying dotAGE as the second in-genre management-roguelite anchor, so it slotted into Approach 1 at its 86 draft peak and Approach 2 at 85, both uncapped with no displacement. It also teaches Approach 4 meta-progression through the Smoldering City tree and a soft Approach 3 through the three-clan mix, but I left both unslotted, because dotAGE already carries the in-genre proof there and Approach 4 is at its cap.
- **rev 6, 2026-07-01.** Removed the per-section count cap. A reference now earns a section slot by clearing the 80 per-lever floor without being redundant, with no fixed count, which makes the rubric consistent with what the doc already did, because Approaches 4 and 6 already ran four references each. Redundancy replaces the headcount as the anti-sprawl valve, so a game clearing 80 is still an Appendix cut when a listed reference teaches its lever-lesson more directly. This re-grounds rev 5's Approach 4 call, because Against the Storm stays out of Approach 4 on redundancy alone now, since dotAGE covers the in-genre meta-tree and Dead Cells covers pool-expansion, not because of a cap. Updated the skill to drop the cap.
- **rev 7, 2026-07-05.** Linked the Design principles to the new cross-game lessons doc
  (`docs/roguelite-replayability-lessons.md`), which argues each principle in full with per-game
  evidence. The list here stays the one-line rubric of record for scoring, principles land and get
  argued in the lessons doc first (via `/lobcorp-game-debrief`), and a principle graduates into
  this list only through `/lobcorp-replayability-eval`, logged here.

## Syncing

This document lives in two places: `docs/roguelite-replayability-approaches.md` in the repo (the canonical body) and a twin in the Obsidian vault (the same body, plus YAML frontmatter). Edit either one, then run `/sync-design-doc` to reconcile, because it diffs the two bodies, propagates the newer one, and stages the vault side for review. Keep substantive design changes here, so the repo history captures them.
