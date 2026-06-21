# Lobotomy Corporation — Content-Authoring Tooling Opportunities

*A code-derived audit of where custom tooling would let designers author & balance content
end-to-end without an engineer touching code. Target: **external tooling that exports mods.***

> Developer-facing analysis doc. Findings are derived from the decompiled game source under
> `external/decompiled/Assembly-CSharp/` (not committed; pulled from the private DLL repo). Line
> numbers reflect that decompilation and may drift if the game updates.

---

## Reconciliation — rev 2 (blind brainstorm + deep probes)

A separate, source-blind brainstorm was reconciled against rev 1, and two priority probes went back
into the code. Treating rev 1 as critically as the blind list, here is what moved.

### Changelog vs rev 1

**Corrected (my own error):** rev 1's QW1 evidence claimed a missing `domain@key` localization
lookup "returns the raw key." It does **not** — `LocalizeTextDataModel.GetText` returns the literal
`"UNKNOWN"` (`LocalizeTextDataModel.cs:39-50`). This *strengthens* QW1: a missing key shows as
`UNKNOWN` in-game with no hint of which key.

**Added — SIM (headless work-balance simulator), now rank 2.** The biggest substantive change. Rev 1
was authoring-heavy and under-served the metric's *balance* half (only BB3 touched it). Probe A1
proved the work-resolution formula is small, pure, and data-fed — a Monte-Carlo simulator is a clean
`[EXTERNAL]` reimplement, not the "drift hazard" the brainstorm feared. This is exactly the
no-code-smell QA tool a pain-hunting pass under-weights.

**Added — DIFF (semantic content-diff), DASH (content-health dashboard), LLM (NL→stat-block layer),
and a localization-editor depth upgrade** (glossary/length/side-by-side). All `[EXTERNAL]`, all thin
layers over the QW1/SIM/QW3 data model. See the Part B table.

**Confirmed negative — deterministic seed-replay is INFEASIBLE as an external tool** (Probe A2). A
clear, well-grounded "no": ~1,045 unseeded `UnityEngine.Random` sites, no `Random.InitState` anywhere,
frame-rate-dependent work ticks. XL `[INTERNAL]` refactor only.

**New context — a rich in-game cheat console exists** (`ConsoleCommand.cs`, 6 namespaces, dozens of
commands) that rev 1 missed. It already delivers a lot of *in-game* QA state manipulation, which
reshapes the "sandbox scene" idea (see #4) — but it is not a content-authoring or external surface.

**Unchanged / survived:** QW1-3, BB1-3, the feasibility cliff, and every anti-recommendation
(skill-trigger UI cut, boss-DSL, ordeal-needs-hook, `Resources`-baked tables) hold up under
re-examination.

### Probe A1 — headless balance simulation: **EXTERNAL (reimplement), low drift risk**

The full per-tick path: `CreatureModel.GetWorkSuccessProb` (`CreatureModel.cs:1339-1361`, pure) →
bonus assembly + roll in `UseSkill.ProcessWorkTick` (`UseSkill.cs:571-612`) → grade via
`FeelingStateCubeBounds.CalculateFeelingState` (`FeelingStateCubeBounds.cs:5`, pure over XML bounds) →
PE via `EnergyModel.instance.AddEnergy(successCount)`. Stat→level is the pure static
`AgentModel.CalculateStatLevel` (`AgentModel.cs:509`). **No singletons in the core probability calc.**
The only non-isolable pieces: ~8 creatures override `TranformWorkProb` (Bald, BloodyTree, LookAtMe,
MagicalGirl, Mhz_1_76, Nothing, ShyThing, SlimeGirl — flag them) and PE *assignment* needs the
`EnergyModel` singleton (but the simulator only needs the predicted *count*, not to write it). The
formula's accuracy boundary is the same script cliff from the headline finding. → **SIM** above.

### Probe A2 — deterministic seed-replay: **IMPRACTICAL / INTERNAL-only (XL)**

`UseSkill.cs:599` rolls `Random.value` with no seed; `RandomEventManager.cs:562` likewise. Census:
**~1,045 `UnityEngine.Random` call sites, zero `Random.InitState`** anywhere; `System.Random` is
effectively unused in gameplay. Worse, determinism is broken even *with* a seed: work ticks advance on
`workProgress += Time.deltaTime * workSpeed` (`UseSkill.cs:117`, frame-rate dependent), plus unordered
`Dictionary.Values` iteration (`RandomEventManager.cs:133`) and 121+ coroutine timing points.
`StorySeedUI.cs` is UI-only — no real seed facility. Exact replay would need an XL source refactor
(route all RNG through one seeded API, replace frame timing with a counter, sort iterations). A
post-hoc "blessed seed" notes list is the only EXTERNAL option, and it is weak. **Verdict: don't.**

### Part B — adjudication of the blind list

| # | Blind item | Verdict | Reasoning (code-grounded) |
|---|---|---|---|
| 1 | Validator w/ referential integrity | **COVERED** | = QW1 exactly. |
| 2 | Abnormality editor + workProb heatmap | **COVERED** | = BB1 (editor) + QW3 (heatmap over `CreatureWorkProbTable`). |
| 3 | Behavior-class scaffold/codegen | **COVERED** | = BB2 (`<script>` → `CreatureBase` subclass via reflection). |
| 4 | Isolated sandbox/test scene | **INFEASIBLE as clean EXTERNAL** | `CreatureManager.AddCreature_Mod` (`CreatureManager.cs:981-1064`) needs a live `Sefira`+room+`MapGraph`; `SceneTester.cs` is an empty stub; `InitGame` opens 19 rooms. In-game cheat console already covers much QA. A mod *can* add a scene but can't spawn one creature in isolation → `[INTERNAL]`, M, low. |
| 5 | Deterministic seed+replay | **INFEASIBLE/INTERNAL** | Probe A2 — XL refactor; impractical externally. |
| 6 | Headless balance sim + outlier dashboard | **ADD** | sim = **SIM** (rank 2); outlier dashboard = **DASH** (rank 8). |
| 7 | Localization editor (glossary/length/side-by-side) | **ADD (modest)** | Loc model is a pure `id→string` dict with no metadata (`LocalizeTextDataModel.cs:10`); glossary, char-count overflow *warnings*, and source/target diff are 100% EXTERNAL pre-write checks. Runtime layout-overflow enforcement needs TMP width calc → `[INTERNAL]`, cut that part. Folds into QW1/BB1 loc handling. |
| 8 | Validation as CI/build gate | **COVERED** | = QW1 run as a git/CI hook (a deployment mode, not a new tool). |
| 9 | Semantic content-diff for PR review | **ADD** | Content is XML with a schema QW1 already parses; rendering "Insight success 0.6→0.7" instead of XML noise is a thin EXTERNAL layer. High value for the org's PR flow → **DIFF** (rank 6), EXTERNAL, S–M. |
| 10 | LLM-assisted authoring (NL→stat block) | **ADD (composes)** | Not a code-feasibility question; it composes because the schema is known (BB1) and a similar-risk-creature corpus is extractable (QW3) to constrain suggestions. EXTERNAL layer atop BB1 → **LLM** (rank 10). |
| 11 | Producer content-health dashboard | **ADD** | Aggregates QW1 (validation/loc/art coverage) + SIM (balance outliers) + QW3 (stat distributions). EXTERNAL read-only view → **DASH** (rank 8). |

---

## Context & method

I mapped the decompiled game (`external/decompiled/Assembly-CSharp/`, ~2,000 root classes) plus the
BaseMod lib, then fanned out 14 read-only explorers across every authored system, ran an
**adversarial verification pass** over every load-bearing feasibility claim, and a completeness
critic for blind spots. I then **personally re-read** the claims that flip a feasibility verdict.
The adversarial pass mattered: it overturned 10 claims (skill-trigger XML is parsed-but-never-run,
ordeal→creature bindings are hardcoded, "boss action DSL" rested on a fabricated class count, etc.),
which is reflected below. Everything cited is **observed** in the code unless tagged *(inferred)*.

The metric throughout: **how much content work this removes from an engineer's plate.**

---

## The headline finding (read this first)

Lobotomy Corporation has a **sharp feasibility cliff**, and it runs straight through the middle of
every abnormality:

- **A creature/EGO's entire numeric & metadata surface is XML, loaded from per-mod folders.**
  `CreatureDataLoader.LoadCreatureStat()` (`CreatureDataLoader.cs:233-538`) parses HP, per-attribute
  work-success probabilities (`workProb` R/W/B/P × level 1-5), the 4-type defense matrix
  (`defense`/`defenseElement`), observation/encyclopedia rewards (`observeInfo`), qliphoth counter
  max, work cooltime/speed, escape flag, and the EGO drop table (`equipment equipId/level/cost/prob`)
  — all from XML. `EquipmentDataLoader.LoadEquips()` does the same for EGO damage/defense/range/
  requirements. Both loaders iterate `Add_On.instance.ModList` and read loose `.xml`/`.txt` from each
  mod's content dirs (`CreatureDataLoader.cs:44-220`, `EquipmentDataLoader.cs:32-96`).

- **A creature's *behavior* is a hardcoded C# class.** The XML carries `<script>SomeClassName</script>`
  (`CreatureDataLoader.cs:236-238`), and the engine resolves it by **reflection** —
  `ExtenionUtil.GetTypeInstance<CreatureBase>(data_Mod.script)` (`CreatureManager.cs:890`,
  `OrdealManager.cs:290`, `SpecialEventManager.cs:290`). There are **115 direct `CreatureBase`
  subclasses** (plus 86 `*Weapon`, 56 `*Buf`, 18 `*Skill` classes). Each hand-implements escape
  patterns, work-trigger abilities, attack logic, phase transitions — none of it data-driven.

**So:** a designer can author a *complete, balanced, shippable* abnormality or EGO item — stats,
defenses, work odds, drop tables, encyclopedia text, art, audio — **with zero engineering**, *as long
as it reuses an existing `<script>` (or has no special behavior).* Novel mechanics still need a C#
DLL. This boundary is the single most important fact for tooling: **tool the data layer aggressively;
the behavior layer is codegen-assist at best.**

A second, subtler cliff: **two data paths exist, and only one is moddable cleanly.**
- *Loose-file path* (moddable, EXTERNAL-friendly): creatures, equipment, localization, art, audio,
  in-game mod-option UI — all scanned from mod folders via `Add_On`.
- *Baked-in path* (`Resources.Load("xml/...")`, **not** loose-file moddable): the global/balance
  tables in `GameStaticDataLoader.cs` — faction hostility, PE-box reward grades, ordeal spawn
  tables, suppression-squad actions, encyclopedia narration/lyrics. Editing these requires a Harmony
  hook or asset-bundle replacement, **not** a clean XML drop-in.

And a recurring pain that quietly dominates everything: **loaders swallow malformed content.**
Every loader wraps its body in `catch (Exception ex) { ModDebug.Log("...error..." ); }`
(`CreatureDataLoader.cs:227-230`, `EquipmentDataLoader.cs` Load). A typo'd creature XML doesn't error
— it **silently fails to load**, with feedback only in a log file. There is **no schema, no
validation, no referential checking** anywhere in the content pipeline.

---

## Verified feasibility map

| System | Verdict | Authoring path | Tool-able EXTERNALLY? |
|---|---|---|---|
| Abnormality stats (HP, workProb, defense, observe, qliphoth, drops, speed) | **data-driven** | `Creature/CreatureList/*.xml` + `Creature/CreatureGen/*.xml` | ✅ fully |
| EGO equipment (damage, defense, range, requirements, sprite) | **data-driven** | `Equipment/txts/*.xml` | ✅ fully |
| Localization (UI/creature/EGO/story text, `domain@key`) | **data-driven** | `Language/Localize/{lang}/…` + mod dirs | ✅ fully |
| Custom art / audio | **data-driven** | `Creature/Portrait/*.png`, `BaseModArtWork/`, `BaseModAudioClip/` | ✅ fully |
| In-game mod config UI (Toggle/Slider) | **data-driven** | `Info/GlobalInfo.xml` `/info/Option[@Type]` | ✅ fully |
| Abnormality **behavior** (escape/skills/attacks) | **hardcoded** | 115 `CreatureBase` subclasses, reflection by `<script>` | ⚠️ codegen-assist only (DLL) |
| Skill-trigger *execution* | **hardcoded** | `<skillTrigger>` XML is **parsed but never run** (`SkillTriggerCheck.roomEnterCheck()` has 0 callers); runtime is per-class `OnEnterRoom` overrides | ⚠️ trap — don't tool the XML |
| Ordeal → creature **bindings** | **hardcoded** | `static int[] ids` per ordeal class (`BugOrdeal.cs:10`, `MachineOrdeal.cs:10`, …) | ⚠️ needs code hook |
| Story scene **triggers** | **hardcoded** | switch/case + literal story IDs in `StorySceneController` (scene *content* is XML) | ⚠️ partial |
| Global balance tables (factions, reward grades, ordeal spawn rates, narration) | data-shaped but **baked** | `GameStaticDataLoader` `Resources.Load("xml/...")` | ⚠️ needs Harmony hook |
| Boss fights | **hardcoded** | ~8 hand-written action classes + `static` damage fields per boss | ❌ bespoke |
| Save state | **opaque binary** | `BinaryFormatter`, magic-string keys, per-model `GetSaveData/LoadData` | ✅ via reverse-engineering |

---

## Ranked opportunities

Scores are 1-100 against the project's context: an Open-Lobotomy org whose principles are
**modder-first / low-barrier-to-entry / trustworthy-foundations**, where the export target is the LMM
mod-folder format and an external CLI/tool ecosystem (TheSilentOrchestrator) already exists to host this.

### QUICK WINS — EXTERNAL, ship-this-quarter, foundational

**QW1 — Mod content validator & linter (CLI + XSD) · `[EXTERNAL]` · effort S–M · leverage HIGH · 96/100**
A standalone tool that parses a mod folder *exactly as the game's loaders do* and surfaces the errors
the game **silently swallows**, plus referential integrity: does every `equipId` a creature references
exist in `EquipmentTypeList`? Does every `<script>` name resolve to a real `CreatureBase` type? Do
localization keys (`domain@key`) referenced by the content exist? Are creature/equipment IDs unique
across `BaseList` + all mods (respecting `modid`/`LcIdLong` namespacing)?
- *Evidence:* silent `catch`→`ModDebug.Log` in every loader (`CreatureDataLoader.cs:227-230`);
  cross-ref `equipId → EquipmentTypeList.instance.GetData(id)` (`CreatureDataLoader.cs:482`, returns
  null and `continue`s on miss); `domain@key` lookup returns the literal string `"UNKNOWN"` on miss
  (`LocalizeTextDataModel.cs:39-50`) — a missing key renders as `UNKNOWN` in-game, not even the key
  name. *(rev 2 correction: rev 1 said "returns the raw key" — verified wrong against the source.)*
- *Pain removed:* today the only way to know your mod is broken is to launch the game and read a log.
- *Why #1:* lowest effort, **everything else depends on it**, and the dominant authoring pain the code
  reveals is *silent failure*, not typing XML. Directly serves "trustworthy foundations."
- *Dedup of:* "Mod Content Validator", "XML Schema (XSD) Spec & Validator", "ID Collision Detector",
  "Equipment XML Validator", "Localization Key Validator", "Creature Script Validator".

**QW2 — Mod scaffolder / `new-mod` generator · `[EXTERNAL]` · effort S · leverage HIGH · 93/100**
Emits the exact loadable folder skeleton — `Info/{lang}/info.xml` manifest (`/info/name`,
`/info/descs/desc`, `modid`), `Creature/CreatureGen`, `Creature/CreatureList`, `Equipment/txts`,
`BaseModArtWork/`, `Localize/{lang}/` — pre-seeded with one valid example creature + EGO that loads
on first launch.
- *Evidence:* dir names the loaders scan (`CreatureDataLoader.cs:123-136`, `EquipmentDataLoader.cs:35`);
  manifest parse (`ModInfo.cs Init`, `/Info/{lang}/info.xml`).
- *Pain removed:* the "piece it together from scattered gists" barrier — the #2 org principle, verbatim.

**QW3 — Base-game extractor → tuning workbook → XML emitter · `[EXTERNAL]` · effort M · leverage VERY-HIGH · 92/100**
Round-trips shipped content: read `BaseList.txt` + `Equipment.txt` into an editable
spreadsheet/JSON, let a designer tune the whole balance surface, **re-emit XML** (the loaders only
accept `.xml`/`.txt` — verified, no JSON/YAML path exists, so the tool's internal format is free but
the *output* must be XML). Gives designers the entire base game as a tuning surface and a mod that
overrides it.
- *Evidence:* `EquipmentDataLoader.cs:15-26` extracts `Resources` → `BaseMod/BaseEquipment.txt` on
  first run; creature override-by-file-replacement (`CreatureDataLoader.cs:176-187`).
- *Dedup of:* "Creature Data Exporter", "Balance Tuning Workbook (CSV)", "Magic Number Externalization",
  "Work Probability Matrix/Heatmap", "Defense Matrix Editor".

**SIM — Headless work-balance simulator · `[EXTERNAL]` (reimplement) · effort S–M · leverage VERY-HIGH · 91/100**
*(Added in rev 2 — see the reconciliation section. The original report under-served the "balance" half
of the metric; this is the fix.)* Estimate a creature's real work-success rate and PE(enkephalin)
income from its stats **before anyone plays**, by reimplementing the work-resolution formula and
Monte-Carloing it. The whole base formula is pure + data-driven and isolable:
- base prob `CreatureModel.GetWorkSuccessProb` (`CreatureModel.cs:1339-1361`) = `workProbTable.GetWorkProb(rwbpType, CalculateStatLevel(agentStat))` — no singletons;
- `AgentModel.CalculateStatLevel` (`AgentModel.cs:509`) is a pure threshold function (`<30→1, <45→2, <65→3, <85→4, else 5`);
- per-tick assembly + roll `UseSkill.ProcessWorkTick` (`UseSkill.cs:571-612`): `+observeBonus/100 +script.OnBonusWorkProb/100 +agent.workProb/500 +equip/500 +buffs/100`, clamp `0.95`, `−qliphothReduction`, roll `Random.value`;
- grade = `FeelingStateCubeBounds.CalculateFeelingState(successCount)` (`FeelingStateCubeBounds.cs:5`) — pure, over XML-loaded `upperBounds`; PE ≈ successCount (`EnergyModel.instance.AddEnergy`).
- *Accuracy boundary (honest):* exact for the ~100 creatures with no script override; only **~8 creatures** override `TranformWorkProb` (Bald, BloodyTree, LookAtMe, MagicalGirl, Mhz_1_76, Nothing, ShyThing, SlimeGirl) — flag those. Take agent/equipment/buff bonuses as **input parameters** rather than reimplementing 50+ bonus classes.
- *Why not a drift hazard:* the core is ~25 lines, mostly data-fed; the only loose magic constants are the `/100`,`/500`,`0.95` divisors. Pairs directly with QW3 — edit stats, see outcomes, iterate, never launch the game.

### BIG BETS — EXTERNAL, higher effort, transformational leverage

**BB1 — Abnormality + EGO authoring GUI with one-click mod export · `[EXTERNAL]` · effort L · leverage VERY-HIGH · 90/100**
The flagship. A forms-over-XML editor: author a full abnormality (every field in the feasibility-map
row) and EGO item, with **autocomplete on referenced IDs** (equipment, skill, localization keys), live
validation (QW1 embedded), a `<script>` dropdown listing existing reusable behaviors + a "no special
behavior" option, and **export to a loadable LMM mod folder** (QW2 embedded).
- *Evidence:* the complete XML schema is knowable from `LoadCreatureStat`/`LoadEquips`; fields are
  literal — e.g. creature equip list is `equipMakeInfos: List<CreatureEquipmentMakeInfo>`
  (`CreatureTypeInfo.cs:236`), `EquipmentTypeInfo` exposes `damageInfos[]`/`defenseInfo`/`range` and
  **no** `cost` field (cost lives on `CreatureEquipmentMakeInfo`).
- *Feasibility boundary (state it honestly):* this authors *everything except* novel `<script>`
  behavior. That still covers a large fraction of real content (re-skins, stat variants, new
  work-only abnormalities, EGO gear).
- *Dedup of:* ~15 explorer items ("Interactive Authoring GUI", "Abnormality Stat Editor", "Equipment
  Exporter", "Creature Balance Dashboard", "Skill Trigger Authoring UI" — the last must be **cut**:
  the trigger XML it would edit is never executed).

**BB2 — Behavior-script template generator + net35 DLL build pipeline · `[EXTERNAL]` (codegen) · effort L · leverage HIGH · 78/100**
For the hardcoded half. Generate a `CreatureBase` (or `EquipmentScriptBase`) subclass stub from a
chosen archetype (escape pattern, on-work effect, skill proc, damage multiplier), with the lifecycle
overrides pre-wired (`OnEnterRoom`/`UniqueEscape`/`GetDamageFactor`/`OnReleaseWork`), build it to a
net35 DLL with the `Harmony_Patch`/`ModInitializer` entry point LMM looks for (`Add_On.cs:106-166`),
and link it to the XML `<script>` field.
- *Honest scope:* this **shrinks** the engineer requirement for custom mechanics; it does not remove
  it. It is codegen, **not** a behavior VM — the verifier confirmed no DSL/interpreter exists; behavior
  is polymorphic C# resolved by reflection. A true declarative behavior DSL that compiles to a
  subclass is the XL frontier below.
- *Dedup of:* "Creature Behavior Script Code Generator", "Equipment Script Template Generator",
  "Harmony Patch Code Generator".

**BB3 — Save inspector / converter + balancing test harness · `[EXTERNAL]` · effort L · leverage HIGH · 74/100**
Reverse-engineer the `BinaryFormatter` save (schemas are fully readable from each model's
`GetSaveData/LoadData`, `GlobalGameManager.cs:496-546`), expose it as editable JSON, and let QA/
designers **jump to any game-day/state** to balance encounters without grinding there. Serves the
*balancing* half of the metric, which the authoring tools don't.
- *Evidence:* `SaveUtil.cs:98-130` BinaryFormatter; magic-string keys `'agents'`/`'creatures'`;
  `ModSaveUtil.cs:20-44` mod data is `Dictionary<string,object>`.
- *Caveat:* BinaryFormatter type-fidelity makes a writer fiddlier than a reader; ship read/inspect
  first, edit second.

### Frontier (named, but not recommended yet)

**Creature-behavior DSL / visual state-machine compiler · `[EXTERNAL]` · effort XL · leverage VERY-HIGH · 48/100**
The explorers' most ambitious idea. Real ceiling, but: (a) no existing interpreter to build on —
it's pure greenfield compiler work; (b) 115 bespoke subclasses set a brutally high bar for "expressive
enough to be worth it." BB2's templates capture most of the value at a fraction of the risk. Park it.

---

## Anti-recommendations

- **Boss fights (Binah/Geburah/Kether/White Night) — don't build a boss DSL.** Verification killed
  the premise: Binah has ~8 hand-written action classes (not 60+), instantiated by constructor calls in
  phase classes, with damage as `static` fields in `BinahStaticData.cs`. Small N, deeply bespoke
  per-action logic. A config/codegen layer would cost more than it saves. *Already irreducibly code.*
- **Per-creature special mechanics — don't try to "data-drive" them.** 115 polymorphic `CreatureBase`
  subclasses. The right intervention is BB2 (scaffold the boilerplate), not a data schema.
- **Skill-trigger XML editor — actively misleading; cut it.** `<skillTrigger>` is parsed into
  `SkillTriggerCheck` lists that **nothing reads** (`roomEnterCheck()` has zero callers). A tool here
  would author data the game ignores.
- **Ordeal authoring (clean EXTERNAL version) — blocked without a code hook.** Creature *metadata* is
  XML, but ordeal→creature bindings are `static int[]` literals per class and spawn-rate tables are
  `Resources`-baked. A Harmony patch adding a loose-file ordeal loader is a prerequisite — that's an
  `[INTERNAL]`-adjacent enabler, not a drop-in tool.
- **Global balance tables (factions/reward-grades/narration) — same blocker.** `Resources.Load("xml/…")`
  is baked into the asset bundle; not loose-file moddable without a Harmony override.
- **The data loaders & runtime data structures themselves — already clean, leave them.**
  `CreatureWorkProbTable`, `DefenseTable`, `ObserveInfoData`, the `domain@key` localization dictionary
  are tidy generic containers. The gap is *authoring/validation on top*, which QW1/BB1 fill — not a
  redesign of these.

---

## The unglamorous truth

The highest-ROI item is **not** the authoring GUI and definitely not a behavior DSL. It's **QW1, the
validator** — a few weeks of work that reads the loaders' logic and reports what the game refuses to
tell you. The code's loudest, most repeated signal isn't "XML is hard to write"; it's **"malformed
content fails silently"** (the same `catch → ModDebug.Log` swallow in every loader). Fix the feedback
loop first; every richer tool is easier and safer once a designer can trust that "it validated" means
"it will load."

---

## Scoreboard (rev 2 — ordinal; rank matters more than the digits)

| Rank | Tool | Tag | Effort | Leverage | Δ from rev 1 |
|---|---|---|---|---|---|
| 1 | QW1 · Mod content validator & linter | EXTERNAL | S–M | HIGH | — (loc evidence corrected) |
| 2 | **SIM · Headless work-balance simulator** | EXTERNAL (reimpl) | S–M | VERY-HIGH | **NEW** |
| 3 | QW2 · Mod scaffolder / `new-mod` | EXTERNAL | S | HIGH | — |
| 4 | QW3 · Extractor → tuning workbook → XML | EXTERNAL | M | VERY-HIGH | — |
| 5 | BB1 · Abnormality + EGO authoring GUI | EXTERNAL | L | VERY-HIGH | — |
| 6 | **DIFF · Semantic content-diff for PR review** | EXTERNAL | S–M | MED-HIGH | **NEW** |
| 7 | BB2 · Behavior-script template + DLL build | EXTERNAL (codegen) | L | HIGH | — |
| 8 | **DASH · Producer content-health dashboard** | EXTERNAL | M | MED-HIGH | **NEW** |
| 9 | BB3 · Save inspector / converter + harness | EXTERNAL | L | HIGH | — |
| 10 | **LLM · NL→stat-block authoring layer (atop BB1)** | EXTERNAL | M | MED | **NEW** |
| — | Behavior DSL / visual compiler | EXTERNAL | XL | VERY-HIGH | parked |
| ✗ | Deterministic seed-replay (Probe A2) | INTERNAL | XL | LOW | **INFEASIBLE as EXTERNAL** |
| ✗ | Isolated sandbox/test scene (blind #4) | INTERNAL | M | LOW | partly covered by in-game cheat console |

**Recommended build order:** **QW1 + SIM first** — the two legs of the stool (QW1 = trustworthy
authoring, SIM = balancing without play; both standalone, both unblock everything). Then QW2 → QW3
(these + QW1 *are* most of BB1) → BB1. DIFF/DASH are thin layers over QW1+SIM+QW3 data — build once
those exist. BB2/BB3 last. Skip the DSL and replay.

---

## Appendix — the export target (reverse-engineered mod folder)

A loadable LMM mod is a directory under the mods root containing:

```
Info/{lang}/info.xml        # manifest: /info/name, /info/descs/desc; modid drives LcIdLong namespacing
Info/GlobalInfo.xml         # optional in-game config UI: /info/Option[@Type=Toggle|Slider]
Creature/CreatureGen/*.xml  # /All/add, /All/remove — which creature IDs this mod enables
Creature/CreatureList/*.xml # /creature_list/creature — full stat blocks (LoadCreatureStat schema)
Creature/Portrait/*.png     # portraits (File.ReadAllBytes + Sprite.Create)
Equipment/txts/*.xml        # EGO weapons/armor/gifts (LoadEquips schema)
Localize/{lang}/*.xml       # <localize><text id="">value</text></localize>
BaseModArtWork/ , BaseModAudioClip/   # sprites / .wav/.mp3
*.dll                       # optional: Harmony_Patch or ModInitializer entry point for <script> behavior
```

This folder is exactly what QW1 validates, QW2 scaffolds, and BB1 exports.

*Verification note:* feasibility verdicts were adversarially re-checked; refuted claims were corrected
or cut. Decompilation artifacts (renamed locals, `item6`-style names) are present but did not affect
any schema or path cited here — those were confirmed against literal loader code.
