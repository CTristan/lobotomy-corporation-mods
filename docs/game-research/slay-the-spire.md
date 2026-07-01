# Slay the Spire

- **Genre:** Single-player deckbuilding roguelike (draft a deck, climb three acts, fight elites and bosses).
- **Why a candidate:** It is the master map's Approach 1 keystone (92) and the model for the conditioned-draft POC, so it is the game whose draft mechanic the build-first POC has to reproduce.
- **Confidence:** medium — the levers are well understood from the master map, but the POC-level detail (how much the *offer* is conditioned versus the player supplying the synergy) is not yet observed firsthand.
- **Readiness:** researching

## Mechanic inventory *(relaxed)*

Seeded from the master map's existing read, to verify and deepen by play:
- **Contextual card value.** A card that is dead weight in one deck is a build-around in another, so the variance lives in how the pieces interact, not in the pieces themselves.
- **Relics swing archetypes.** A single relic can flip the value of a whole card archetype, which is a major source of run-to-run divergence.
- **Card-reward draft.** After most fights you pick one card from a small offer (or skip), which is the between-fight draft structure Blue Prince mirrors at the day level.
- **Four asymmetric characters.** Each character is a different *kind* of run (distinct starting deck, relic, and core mechanic), not a stat swap.
- **Meta-progression (lighter lever).** Ascension levels and card-pool unlocks widen difficulty and options across runs, but the master map deliberately does not list StS under Approach 4, because Hades, dotAGE, Dead Cells, and Rogue Legacy teach that lever more directly.

## Approach mapping *(graduating)*

- **1 drafted loadout + interacting pieces:** keystone (92). Card value is contextual and relics swing whole archetypes, so this is the lever the LobCorp blind-pick POC has to reproduce.
- **2 procedural / affixed content:** not this game's lever. StS varies which cards you are *offered*, not the content of encounters through rolled affixes, so Approach 2 belongs to Hades, Diablo, and the management proofs.
- **3 asymmetric starts / factions:** strong (90 here). Four characters each reshape strategy from turn one, which is the depth-per-faction lesson for the patron-Sephirah mod.
- **4 meta-progression / unlock divergence:** present but lighter, and redundant. Ascension and card unlocks widen difficulty more than they diverge play, so the master map's Approach 4 references cover the lever better.
- **5 structural run-restructuring:** not observed. The run shape (three acts, elite and boss cadence) is fixed; the variance is in contents, not structure.
- **6 emergent systems + adaptive director:** not observed. There is no state-reading director; pacing is a fixed act curve.

## Answered questions *(relaxed)*

(none yet)

## Open questions *(relaxed)*

- **Are card and relic offers conditioned on your current deck, or is the offer pool flat and the player supplies the synergy?** This is the load-bearing POC question, because it decides whether LobCorp's blind-pick draft needs offer-conditioning logic or just a pool of contextually-valued abnormalities plus a few archetype-swinging modifiers.

## In-game task tracker *(relaxed)*

| Task | Status | Findings |
|---|---|---|
| Play 2–3 runs. At every card reward, note whether the three offered cards look tied to your current deck/archetype or just rarity-weighted from the character pool; do the same for shop and relic offers. Record one case where *you* supplied the synergy (picked a card that only worked because of what you already held) and one case where an offer or relic itself swung an archetype. | assigned | |

## Integration hypotheses & what won't transfer *(graduating)*

- **Provisional (pending the offer-conditioning task).** If StS gets its variance from a mostly flat offer pool plus player synergy-seeking and archetype-swinging relics, then the LobCorp POC does not need to build complex offer-conditioning; it needs a small abnormality pool whose value is contextual on what you already hold, plus a few "relic-like" modifiers that flip whole archetypes. The scale won't transfer, because StS leans on a 350-plus card pool to stay fresh and LobCorp's abnormality roster is an order of magnitude smaller, so each abnormality has to carry more recombination.

## Session log *(relaxed)*

- 2026-07-01 scaffolded from the master map's Approach 1/3 read; queued the offer-conditioning task as the first POC-level unknown.
