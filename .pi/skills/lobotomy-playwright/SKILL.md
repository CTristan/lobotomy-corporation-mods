---
name: lobotomy-playwright
description: CLI bridge for observing a running Lobotomy Corporation game instance. Query game state, wait for events, capture screenshots, and manage the game lifecycle.
---

# LobotomyPlaywright

CLI tool for observing and managing a running Lobotomy Corporation game instance. All commands use `dotnet playwright <command>`.

## Rules

1. **Always stop the game when finished.** Run `dotnet playwright stop` before completing your response — whether your task succeeded, failed, or you hit an error. Leaving the game running wastes resources and blocks future launches.
2. **You cannot view screenshots.** You are a text-only agent. Use `dotnet playwright query ui` to "see" the game's visual/UI state. Screenshots are for human debugging only.
3. **Wait for READY status before querying.** After `launch`, the game must reach READY status. Use `dotnet playwright status --exit-code` to verify. Queries will fail on loading/title screens.
4. **Deploy before launching** if you changed any plugin code. Sequence: `stop` → `deploy` → `launch`.

## Setup

```bash
dotnet tool restore                        # One-time: install the playwright CLI tool
dotnet playwright find-game                # One-time: auto-detect game path and create config.json
dotnet playwright find-game --check        # Validate existing configuration
```

## Lifecycle Workflow

```bash
dotnet playwright deploy                   # Build and deploy plugin DLLs to game
dotnet playwright launch                   # Launch game and wait for TCP readiness
dotnet playwright status                   # Verify game is READY
dotnet playwright query game               # Query game state
dotnet playwright query agents             # Query agents
dotnet playwright query ui                 # "See" the game UI (primary observation method)
dotnet playwright stop                     # Stop game when done (ALWAYS do this)
```

Quick redeploy after code changes:

```bash
dotnet playwright stop && dotnet playwright deploy && dotnet playwright launch
```

## Command Reference

### find-game

Auto-detect game installation and create configuration.

```bash
dotnet playwright find-game [options]
```

| Option | Description |
|--------|-------------|
| `--path PATH` | Manual game path |
| `--bottle NAME` | CrossOver bottle name |
| `--check` | Validate existing config |
| `--verbose` | Show detailed search output |

### deploy

Build and deploy plugin DLLs to the game directory.

```bash
dotnet playwright deploy [options]
```

| Option | Default | Description |
|--------|---------|-------------|
| `--configuration CONFIG` | Release | Build configuration |
| `--skip-build` | — | Deploy existing DLLs without rebuilding |
| `--dry-run` | — | Show what would be deployed |

Deploys: `LobotomyPlaywright.Plugin.dll` + `HarmonyDebugPanel.dll` → `BepInEx/plugins/`, `RetargetHarmony.dll` → `BepInEx/patchers/`, Harmony interop DLLs → `BepInEx/core/`.

### launch

Launch the game and wait for the plugin to become responsive.

```bash
dotnet playwright launch [options]
```

| Option | Default | Description |
|--------|---------|-------------|
| `--no-wait` | — | Launch without waiting for readiness |
| `--force` | — | Force-kill existing game before launching |
| `--timeout SECONDS` | from config (~120) | Readiness timeout |
| `--host HOST` | localhost | TCP host |
| `--port PORT` | from config (8484) | TCP port |

If the game is already running, `launch` stops it first (graceful then force-kill).

### status

Check the current game and plugin status.

```bash
dotnet playwright status [options]
```

| Option | Default | Description |
|--------|---------|-------------|
| `--json` | — | Output as JSON |
| `--exit-code` | — | Exit non-zero if not READY |
| `--wait-for STATUS` | — | Wait for status: `stopped`, `starting`, `ready`, `unresponsive` |
| `--timeout SECONDS` | 60 | Wait timeout |
| `--poll SECONDS` | 1.0 | Poll interval |
| `--host HOST` | localhost | TCP host |
| `--port PORT` | from config | TCP port |

### stop

Stop the game (graceful TCP shutdown, then force-kill if needed).

```bash
dotnet playwright stop [options]
```

| Option | Default | Description |
|--------|---------|-------------|
| `--force` | — | Skip graceful shutdown |
| `--wait` | — | Wait for processes to exit |
| `--timeout SECONDS` | from config (~10) | Shutdown timeout |
| `--host HOST` | localhost | TCP host |
| `--port PORT` | from config | TCP port |

### query

Query game state. This is the primary way to observe the game.

```bash
dotnet playwright query <target> [id] [options]
```

**Targets:**

| Target | Aliases | ID | Description |
|--------|---------|-----|-------------|
| `agents` | `agent` | int | List all agents or get one by ID |
| `creatures` | `creature`, `abnormalities`, `abnormality` | int | List all abnormalities or get one by ID |
| `game` | `status` | — | Game state overview |
| `departments` | `department`, `sefira`, `sefiras` | — | Department status |
| `ui` | — | — | UI accessibility tree (how you "see" the game) |

**Global options:** `--json`, `--host HOST`, `--port PORT`

**UI-specific options:**

| Option | Default | Description |
|--------|---------|-------------|
| `--depth summary` | — | Show window open/closed states only |
| `--depth full` | (default) | Show windows + child elements |
| `--name WINDOW` | — | Query only a specific window (e.g., `AgentInfoWindow`) |

### read-log

Read BepInEx log files from the game directory.

```bash
dotnet playwright read-log [options]
```

| Option | Default | Description |
|--------|---------|-------------|
| `--file FILE` | LogOutput.log | Specific log file |
| `--tail N` | — | Show last N lines |
| `--filter TEXT` | — | Filter lines containing TEXT (case-insensitive) |
| `--list` | — | List available log files with sizes |

### wait event

Wait for specific game events. Blocks until the first matching event fires or timeout.

```bash
dotnet playwright wait event <event-names...> [options]
```

| Option | Default | Description |
|--------|---------|-------------|
| `--timeout SECONDS` | 60 | Maximum wait time |
| `--json` | — | Output event data as JSON |
| `--host HOST` | localhost | TCP host |
| `--port PORT` | from config | TCP port |

**Available events:** `OnAgentDead`, `OnAgentPanic`, `OnWorkStart`, `OnWorkCoolTimeEnd`, `OnCreatureSuppressed`, `OnEscape`, `OnOrdealStarted`, `OnNextDay`, `OnStageStart`, `OnStageEnd`, `OnGetEGOgift`

Multiple events can be specified — waits for whichever fires first.

### screenshot

Capture a screenshot of the current game state. Note: you cannot view images — use `query ui` instead.

```bash
dotnet playwright screenshot [options]
```

| Option | Default | Description |
|--------|---------|-------------|
| `--format base64\|path` | base64 | Return base64 data or just file path |
| `--display text\|json` | text | Output format |
| `--output PATH` | — | Save decoded image to local path |
| `--host HOST` | localhost | TCP host |
| `--port PORT` | from config | TCP port |

Screenshots saved to `<gamePath>/LobotomyPlaywrightScreenshots/`.

### command

Send commands to manipulate game state or simulate player actions. This is how you interact with the game programmatically.

```bash
dotnet playwright command <action> [options]
```

**Debug Commands (state manipulation):**

```bash
# Set agent stats
dotnet playwright command set-agent-stats --agent <id> [--hp <value>] [--mental <value>] [--fortitude <value>] [--prudence <value>] [--temperance <value>] [--justice <value>]

# Add/remove E.G.O. gifts
dotnet playwright command add-gift --agent <id> --gift <gift-id>
dotnet playwright command remove-gift --agent <id> --gift <gift-id>

# Set qliphoth counter
dotnet playwright command set-qliphoth --creature <id> --counter <value>

# Fill energy
dotnet playwright command fill-energy

# Set game speed (1-5)
dotnet playwright command set-game-speed --speed <1-5>

# Toggle agent invincibility
dotnet playwright command set-agent-invincible --agent <id> [--invincible true|false]
```

**Player Action Simulation:**

```bash
# Pause/unpause game
dotnet playwright command pause
dotnet playwright command unpause

# Assign agent to work
dotnet playwright command assign-work --agent <id> --creature <id> --work <work-type>

# Deploy/recall agents
dotnet playwright command deploy-agent --agent <id> --sefira <department>
dotnet playwright command recall-agent --agent <id>

# Command suppression
dotnet playwright command suppress --creature <id>
```

**Common Options:**

| Option | Default | Description |
|--------|---------|-------------|
| `--host HOST` | localhost | TCP host |
| `--port PORT` | from config | TCP port |

**Work types:** `instinct`, `insight`, `attachment`, `repression`

**Departments:** `BINAH`, `CHESED`, `GEBURAH`, `TIPHERETH`, `NETZACH`, `YESOD`, `MALKUTH`

## Data Model

### Agent
`instanceId`, `name`, `hp`/`maxHp`, `mental`/`maxMental`, `fortitude`, `prudence`, `temperance`, `justice`, `currentSefira`, `state` (IDLE/WORKING/PANIC/STUNNED/DEAD), `weaponId`, `armorId`, `giftIds`, `isDead`, `isPanicking`

### Creature
`instanceId`, `metadataId`, `name`, `riskLevel` (ZAYIN/TETH/HE/WAW/ALEPH), `state`, `qliphothCounter`/`maxQliphothCounter`, `feelingState`, `currentSefira`, `workCount`, `isEscaping`, `isSuppressed`

### Game State
`day`, `gameState` (STOP/PLAYING/etc.), `gameSpeed` (1-2), `energy`/`energyQuota`, `managementStarted`, `isPaused`, `emergencyLevel`, `playTime`, `lobPoints`

### Department
`name`, `sefiraEnum`, `isOpen`, `openLevel`, `agentIds`, `creatureIds`, `officerCount`

## Troubleshooting

- **Connection refused**: Game not running or plugin not loaded. Run `dotnet playwright status` to diagnose.
- **Queries return errors**: Game may be on title/loading screen (not queryable). Wait for management phase or check `dotnet playwright query game` for current state.
- **Deploy fails "path does not exist"**: The CrossOver volume may not be mounted. Run `dotnet playwright find-game` to reconfigure.
