# Plan Index

Tracks all planning documents in the repository. Plans are generated via the `/planning` command.

## Plans

| Status | Plan | File | Progress | Summary |
|--------|------|------|----------|---------|
| Active | LobotomyPlaywright | [`PLAN-LobotomyPlaywright.md`](PLAN-LobotomyPlaywright.md) | 74/194 (38%) | Agent-driven game observability and control bridge |
| Draft | RetargetHarmony Improvements | [`PLAN-RetargetHarmonyRefactor.md`](PLAN-RetargetHarmonyRefactor.md) | 0/17 (0%) | Config dedup, dead code cleanup, test coverage |
| Draft | Unified XML Localization | [`PLAN-Localization.md`](PLAN-Localization.md) | 0/16 (0%) | Localization system for tooling projects |
| Draft | ConfigurationManager | [`PLAN-ConfigurationManager.md`](PLAN-ConfigurationManager.md) | 0/27 (0%) | Shared config API in Common + BepInEx-free ConfigurationManager fork as git submodule |
| Draft | AutoFixture + AutoMoq | [`PLAN-AutoFixture.md`](PLAN-AutoFixture.md) | 0/23 (0%) | Adopt AutoFixture + AutoMoq to reduce test boilerplate and enable anonymous test data |
| Completed | Modder's Guide Examples | [`PLAN-ModdersGuideExamples.md`](PLAN-ModdersGuideExamples.md) | 8/8 phases | Real game code examples for each H2/BepInEx feature in the Modder's Guide |
| Completed | Demo Mod | [`PLAN-DemoMod.md`](PLAN-DemoMod.md) | 20/20 (100%) | Verification & showcase mod proving all guide examples compile and work |
| Completed | DemoMod Showcase Expansion | [`PLAN-DemoModShowcase.md`](PLAN-DemoModShowcase.md) | 17/17 (100%) | Expand DemoMod to 11 features, update guide, distribution zip |

## Status Definitions

| Status | Meaning |
|--------|---------|
| **Draft** | Plan written but work has not started |
| **Active** | Work is in progress |
| **Blocked** | Work paused due to a dependency or unresolved issue |
| **Completed** | All tasks done and verified |
| **Archived** | No longer relevant or superseded by another plan |

## Conventions

- Plan files live at the repo root and are named `PLAN-<Topic>.md` in PascalCase
- Update progress fractions in this index when completing significant milestones
- Move plans to Completed once all tasks are checked off and verified
- Move plans to Archived if they are abandoned or superseded — do not delete them
