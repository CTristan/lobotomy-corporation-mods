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

A working reference for a more replayable LobCorp mod, organized around the distinct approaches to replayability and the games that teach each.

**Ranking criterion:** games are judged by their relevance and applicability to building a LobCorp roguelite mod, which means how directly their lessons can be researched and lifted, not how deep the idea is in the abstract. **Confidence** is second-order: my certainty that the assessment is correct, which can stay high even for a "negative" read (a cautionary anti-pattern, say).

## The problem

A LobCorp campaign converges. The only between-day randomness is the draft-of-three abnormality offering, and it doesn't generate replay value, because:

- It's a **choice**, so you steer rather than adapt to imposed variance.
- Abnormalities are **solved in isolation**, so a different draw never demands a different decision.
- You draft most of the pool over a campaign anyway, so every run lands on the same end-state.

Replayability needs variance that is **imposed**, **forces deviation** from a known-optimal line, and is **recombinant** (it stays novel after you've learned every piece).

## Design principles

These cut across every approach below, because they're how you keep any of them fair and legible.

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
- **Research:** Slay the Spire (92%) is the keystone for contextual piece value. Balatro (84%) shows emergent synergy from a tiny known set, Monster Train (80%) layers synergy further, and Blue Prince (83%) supplies the day-draft structure while doubling as a case study in its failure mode (RNG that blocks applying already-earned knowledge).
- **LobCorp:** the conditioned-draft, blind-pick, lockdown work, which is the POC.

### 2. Procedural / affixed content
- **Lever:** the content itself varies each run, either generated outright or recognizable-but-modified through rolled affixes.
- **Research:** Risk of Rain 2 elite types (85%) and Hades enemy variants (88%) layer affixes onto known enemies, Diablo-style monster affixes (85%) are the canonical mod system, and Spelunky (82%) is the model for fair procedural generation. The Binding of Isaac (85%) is the counter-example, the swinginess to avoid. **dotAGE (86%)** is procedural variance inside a *management* game specifically: a randomized building pool plus a reshuffled tech tree each run.
- **LobCorp:** abnormalities roll traits and affixes per run ("enraged," "symbiotic," shuffled work affinities) so the memorized work-matrix breaks. Keep the rolls legible at containment, because this is exactly where fairness gets tested.

### 3. Asymmetric starts / factions
- **Lever:** runs differ because you begin as a fundamentally different "kind."
- **Research:** Slay the Spire characters (92%), Hades weapons and aspects (88%), Monster Train clans (80%), Darkest Dungeon party comps (87%), and Risk of Rain survivors (85%) all start you as a different kind. **dotAGE (86%)** does it through Elders, asymmetric leaders that each change a run's mechanics, which is a direct genre-proof of the patron-Sephirah idea.
- **LobCorp:** patron-Sephirah factions, where you pick an allegiance that reshapes the whole run (available abnormalities, modified rules, win conditions). We're already building this in the faction-allegiance mod, so it's a replayability engine we've half-built.

### 4. Meta-progression / unlock divergence
- **Lever:** runs diverge because what's *available* expands across many runs, and persistent unlocks reshape play.
- **Research:** Hades' Mirror of Night (88%), Dead Cells' unlock pool (85%), and Rogue Legacy (85%) all expand the option space across runs. **dotAGE (86%)** is the closest working model in the list, because its memory-points and "Elder remembers more" system is a roguelite research tree, near-exactly what's proposed here.
- **LobCorp:** the research tree reimagined as a roguelite unlock web, with abnormalities, E.G.O., and mechanics opening up *across* runs rather than within one. Its job is retention, so that failed runs feel productive, which makes it the glue for the variance levers rather than a standalone.

### 5. Structural run-restructuring
- **Lever:** change the *shape* of a run (objectives, starting conditions, branching), not just its contents.
- **Research:** FTL (88%) varies the run through sectors, ship starts, and branching events. Into the Breach (90%) varies islands, squads, and objectives, and RimWorld scenarios (86%) reshape the starting conditions.
- **LobCorp:** varied run objectives and randomized starting facility conditions, so runs are shaped differently rather than just stocked differently. High ceiling, heavy build.

### 6. Emergent systems + an adaptive director
- **Lever:** replayability from systems that produce unpredictable situations, paced by a director that reads game state.
- **Research:** RimWorld's Storyteller (86%) is the director that paces threats, and the buildable face of emergent colony-sim. Left 4 Dead's AI Director (88%) is the canonical "drama manager" that spawns threats and tunes intensity off player state, which makes it the most directly liftable model for a meltdown/ordeal director. Caves of Qud (80%) supplies procedural *narrative* texture through ex-post-facto history generation: generate the artifacts, back-fill the causality, and let player apophenia finish the story (the cheap, liftable version of deep emergence). **dotAGE (86%)** offers a clean threat-pacing model through its escalating Prophecy events, because each run ramps to threats far harder than the early ones.
- **LobCorp:** a director that schedules meltdowns, ordeals, and events dynamically per run, plus procedurally-generated facility history and lore (see the worked example below).
- **Dwarf Fortress** is a foundational reference here, not a competitive entry. It's the ancestor of this whole approach (full-simulation emergence), but it's effectively unbuildable in a focused mod, and its usable lessons are already captured above by RimWorld (the director) and Qud (cheap procedural history). Study it for inspiration, and reach its value through its refinements.

## Worked example — the Memorial / facility-history system

A concrete feature that fell out of the procedural-narrative-texture idea, anchored on Darkest Dungeon's Graveyard and extended through Qud's history technique. It's worth calling out because one cheap system spans three approaches at once: sticky consequences (DD), procedural narrative texture (Qud), and meta-progression (cross-run persistence).

- **Richer incident artifacts.** A death generates a textured after-action report in LobCorp's corporate-clinical voice (a redacted memo, the agent's final log entry, a colleague's note), back-filled around the mechanical event you already know.
- **Cross-run persistence.** Past runs' catastrophes become fixtures in later runs: a marked sealed-off cell, a memorial plaque, a veteran whose file notes they "survived the [generated incident]," an abnormality log referencing a prior failure. Your history writes the lore your next run is set in.
- **Survivor trauma as story.** Breach-survivors carry narrative quirks tied to a specific recorded event, not just stat-debuffs.
- **Relics from the fallen.** A dead agent's E.G.O. gear, or an abnormality's record, bears the generated mark of who fell to it.
- **Why it's cheap.** Every layer is generated text templated around events that already happened mechanically, so there's no simulation and the player's apophenia does the heavy lifting. It's on-brand too, because LobCorp's native voice *is* incident reports and logs, and the redacted, hidden texture suits the Kurvain aesthetic.

## Prioritization roadmap

Ranked on impact (does it move the core needle?), effort (cost to build solo), and fit (including leverage off work already in progress).

1. **Build first: the conditioned draft (Approach 1).** Lowest effort, high fit, real impact. It gets a replayable loop running and proves it's fun before anything heavier.
2. **High-leverage slot: asymmetric factions (Approach 3).** Recommended, because we're already building the patron-Sephirah mod, so it's high impact at low *marginal* cost and serves two projects at once. The alternative is procedural/affixed content (Approach 2), which has the highest raw impact on the convergence problem but is medium-high effort and touches core behavior, so slot it right after factions as the deep-variance play.
3. **The glue: meta-progression (Approach 4).** Moderate effort, high fit, and it drives retention. Best added once you have a variance lever to progress toward.
4. **Ceiling and scope-trap: structural restructuring (5) and deep emergent simulation (6).** Highest ceilings, highest effort, lowest fit. The full-simulation route especially tends to balloon unbounded, so file it under "if the mod takes off," not near-term. That said, the *cheap* slices of Approach 6 (the director and procedural history) are near-term-feasible. It's only the deep-simulation end that's the trap.

## Notes

- The principles above apply to *every* approach, since they're how you keep any of these fair and legible.
- The strongest mods combine approaches, and the Memorial system is the proof: one cheap feature doing three jobs.
- Games recur across approaches (Hades teaches both factions and meta-progression, for instance), which is expected, so study each through the lens of the approach you're building.
- **dotAGE is the closest holistic analog** to the whole project, a solo-built, turn-based management *roguelite*, which is why it legitimately shows up across Approaches 2, 3, 4, and 6 rather than in one. Study it as a complete reference for the overall shape, not just per-lever.
- **Changes under the applicability criterion:** Dwarf Fortress moved from a competitive entry to a foundational reference (a great idea, but unbuildable, so its value lives in RimWorld and Qud). Left 4 Dead's AI Director was added as the more applicable director model. Caves of Qud rose to 80% once its lesson resolved into the cheap, liftable procedural-history technique.

## Appendix — evaluated but not selected

These games came up during the research but didn't earn a listed reference slot in any approach, judged against the applicability criterion. Sections 4–6 already hold exactly three, and the larger sections list their strongest applicable references, so rather than force a hard fourth-place cutoff and demote genuinely useful entries on thin margins, this appendix collects the games that landed in *no* section, the real cuts.

- **Dwarf Fortress** is the ancestor of the emergent-systems approach, but full-simulation emergence is effectively unbuildable in a focused mod, and its usable lessons are already captured by RimWorld (the director) and Qud (cheap procedural history). It stays in Approach 6 as a foundational reference rather than getting cut outright. Great idea, wrong cost.
- **Library of Ruina** is Project Moon's own deterministic deckbuilder, ranked high early on lineage value. But lineage isn't mechanical applicability, and its actual lesson (deep card interaction in a deterministic system) is taught more directly by Slay the Spire and Balatro, both atop Approach 1. Valuable to you, redundant mechanically.
- **Loop Hero** has one genuinely novel idea (you assemble the gauntlet that then challenges you), but it's a single idea, not a deep system. It's outclassed in drafted-loadout (Slay the Spire, Balatro, Monster Train, and Blue Prince are richer) and in structural-restructuring (FTL, Into the Breach, and RimWorld are more applicable), so it tops no section. Worth one look for the inversion concept, not a primary reference.
- **Inscryption** surfaced in the research and is a superb game, but its strength is a one-time meta-narrative experience, and its roguelite replay mode is secondary. As a *replayability* teacher specifically, the only thing being ranked here, it tops nothing. Right game, wrong question.
- **Returnal** surfaced as a procedural roguelite, but its lessons are either redundant or cautionary. Its fair-procedural-generation trick (hand-built rooms shuffled into fresh orders) is already taught by Spelunky and Into the Breach, and the one system reviewers agree works, the parasites, is just "couple cost to power" made legible, which the Diablo-style affixes already cover. Where Returnal is distinctive it's a warning, because the hidden-benefit items and thin meta-progression make risk illegible in a high-skill game, so skilled players rationally ignore the roguelite layer. The cautionary tale, not the model.

Also surfaced but not individually evaluated: the searches turned up several recent titles (Shogun Showdown, Sol Cesto, and others) that I didn't drill into, because none clearly served one of the six approaches better than the entries already listed. Worth a glance for fresh examples, but none looked like a missing piece.

---

## Approach → tracked issues

> The research above is the stable reference. Everything from here down is the living project layer,
> mapping the approaches to the GitHub issues and recording decisions as the mod evolves.

The Roguelite mode is tracked as epic **#79** ("Replace Challenge Mode with a repeatable roguelite experience"), which blocks 16 child enhancement issues **#150–166** (each titled `[Roguelite] …`), plus a related infrastructure bug **#164**. Every child is a Darkest-Dungeon-inspired enhancement layered on the base mode, so several were authored as consequence, economy, or flavor systems rather than pure variance generators, and a few resist a clean single-approach fit (marked `*`).

| Issue | What it adds | Approach | Memorial |
|---|---|---|---|
| **#79** | Replace Challenge Mode with a repeatable roguelite mode *(epic / container)* | 5 | — |
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

`*` Loose or ambiguous fit, so see Coverage & gaps. Approach legend: **1** drafted loadout, **2** procedural/affixed content, **3** asymmetric starts/factions, **4** meta-progression, **5** structural run-restructuring, **6** emergent systems + director + facility history.

### Coverage & gaps

The 16 children cluster on approaches **4, 5, and 6** (the meta-progression, run-restructuring, and emergent-history/director levers). Three approaches are un-ticketed, and the irony is that they're the ones the Prioritization roadmap above ranks *first*:

- **Approach 1 (the conditioned-draft POC) is un-ticketed at the child level.** The draft, blind-pick, and lockdown core lives in **#79** itself as the base mode, not as a child issue. #161 (recruit draft) and #163 (day-start prep) are loadout-shaped but roster- and economy-scoped, not the abnormality draft.
- **Approach 2 (procedural and affixed *abnormality* content) is un-ticketed.** No issue rolls abnormality traits or affixes, because #151 (panic-type roll) and #161 (recruit-stat roll) randomize other axes.
- **Approach 3 (asymmetric starts and factions) is un-ticketed by design.** This is the patron-Sephirah faction-allegiance mod we're building separately, and #166 is explicitly an *upgrade* system, not factions. See the open questions below.

The literal "adaptive director that schedules meltdowns and ordeals" sub-idea of Approach 6 is only lightly covered, by #156's escalating-pressure meter, even though Approach 6 otherwise carries the most issues (its facility-history and emergent-systems clauses).

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

## Syncing

This document lives in two places: `docs/roguelite-replayability-approaches.md` in the repo (the canonical body) and a twin in the Obsidian vault (the same body, plus YAML frontmatter). Edit either one, then run `/sync-design-doc` to reconcile, because it diffs the two bodies, propagates the newer one, and stages the vault side for review. Keep substantive design changes here, so the repo history captures them.
