# Plan Index

Tracks all planning documents in the repository. Plans are generated via the `/planning` command.

## Plans

| Status | Plan | File | Progress | Summary |
|--------|------|------|----------|---------|
| Active | LobotomyPlaywright | [`PLAN-LobotomyPlaywright.md`](PLAN-LobotomyPlaywright.md) | 61/171 (36%) | Agent-driven game observability and control bridge |
| Active | Unified Debug Panel | [`PLAN-DebugPanel.md`](PLAN-DebugPanel.md) | 95/109 (87%) | BepInEx-agnostic Harmony inspection mod |
| Active | RetargetHarmony Installer | [`PLAN-RetargetHarmonyInstaller.md`](PLAN-RetargetHarmonyInstaller.md) | 24/24 (100%)* | Cross-platform installer/uninstaller GUI |
| Draft | RetargetHarmony Improvements | [`PLAN-RetargetHarmonyRefactor.md`](PLAN-RetargetHarmonyRefactor.md) | 0/17 (0%) | Config dedup, dead code cleanup, test coverage |
| Draft | Non-Destructive Harmony Shim | [`PLAN-NonDestructiveHarmonyShim.md`](PLAN-NonDestructiveHarmonyShim.md) | 0/10 (0%) | Prevent BepInEx from rewriting BaseMods DLLs on disk |
| Draft | Unified XML Localization | [`PLAN-Localization.md`](PLAN-Localization.md) | 0/16 (0%) | Localization system for tooling projects |

\* Tasks complete but manual testing tasks still need to be added.

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
