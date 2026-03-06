---
name: lobotomy-playwright
description: Full observability and control bridge between a pi agent and a running Lobotomy Corporation game. Query game state, wait for events, issue commands, and verify outcomes programmatically.
---

# LobotomyPlaywright

LobotomyPlaywright is a Playwright-inspired system for observing and controlling a running Lobotomy Corporation game instance through a pi agent.

## Important Rules

1. **Always stop the game when finished.** When you are done with the game — whether your task succeeded, failed, or you encountered an error — you **must** run `{baseDir}/scripts/stop.py` before completing your response. Leaving the game running wastes system resources and blocks future launches.

## Prerequisites

The Lobotomy Corporation game must be running with the LobotomyPlaywright BepInEx plugin installed.

### macOS with CrossOver

1. CrossOver must be installed (for running the Windows game on macOS)
   - Download from https://www.codeweavers.com/crossover
   - Default path: `/Applications/CrossOver.app`

2. Game must be installed in a CrossOver bottle

3. BepInEx must be installed in the game directory

### Initial Setup

Run the auto-detection script to create configuration:

```bash
{baseDir}/scripts/find_game.py
```

This will:
- Scan CrossOver bottles for the game installation
- Validate BepInEx installation
- Create `config.json` with game path and settings

For manual configuration, use:

```bash
{baseDir}/scripts/find_game.py --path "/path/to/game" --bottle "BottleName"
```

## Python Dependencies

```bash
pip install -r {baseDir}/requirements.txt
```

## Usage

### Full Lifecycle Workflow

The complete build → deploy → launch → verify → stop workflow:

```bash
# First-time setup (auto-detect game path)
{baseDir}/scripts/find_game.py

# Deploy plugin to game
{baseDir}/scripts/deploy.py

# Launch game and wait for TCP readiness
{baseDir}/scripts/launch.py

# Check status
{baseDir}/scripts/status.py

# Query game state
{baseDir}/scripts/query.py game
{baseDir}/scripts/query.py agents

# Stop game when done
{baseDir}/scripts/stop.py
```

### Quick Redeploy

To rebuild, redeploy, and relaunch the game:

```bash
{baseDir}/scripts/stop.py
{baseDir}/scripts/deploy.py
{baseDir}/scripts/launch.py
{baseDir}/scripts/status.py
```

### Find Game (Auto-Detection)

```bash
{baseDir}/scripts/find_game.py                    # Auto-detect game path
{baseDir}/scripts/find_game.py --path "/path"     # Manual path specification
{baseDir}/scripts/find_game.py --check            # Validate existing config
{baseDir}/scripts/find_game.py --verbose          # Show detailed search output
```

### Deploy Plugin

```bash
{baseDir}/scripts/deploy.py                       # Build and deploy (Release)
{baseDir}/scripts/deploy.py --configuration Debug  # Build Debug config
{baseDir}/scripts/deploy.py --skip-build          # Deploy existing DLLs
{baseDir}/scripts/deploy.py --dry-run             # Show what would be done
```

### Launch Game

```bash
{baseDir}/scripts/launch.py                       # Launch and wait for ready
{baseDir}/scripts/launch.py --no-wait            # Launch without waiting
{baseDir}/scripts/launch.py --force              # Relaunch (kill existing)
{baseDir}/scripts/launch.py --timeout 60         # Custom timeout (seconds)
```

### Check Status

```bash
{baseDir}/scripts/status.py                       # Current status (human-readable)
{baseDir}/scripts/status.py --json                # Status as JSON
{baseDir}/scripts/status.py --exit-code           # Exit non-zero if not READY
{baseDir}/scripts/status.py --wait-for ready     # Wait for READY status
{baseDir}/scripts/status.py --wait-for stopped    # Wait for STOPPED status
```

### Stop Game

```bash
{baseDir}/scripts/stop.py                         # Graceful shutdown, then force kill
{baseDir}/scripts/stop.py --force                 # Skip graceful shutdown
{baseDir}/scripts/stop.py --wait                  # Wait for processes to exit
```

### Query Game State

```bash
{baseDir}/scripts/query.py agents                    # List all agents
{baseDir}/scripts/query.py agents <id>               # Get agent details
{baseDir}/scripts/query.py creatures                # List all abnormalities
{baseDir}/scripts/query.py creatures <id>            # Get creature details
{baseDir}/scripts/query.py game                     # Game state overview
{baseDir}/scripts/query.py departments               # Department status
```

### Query Parameters

- `--json` - Output raw JSON instead of formatted text
- `--host HOST` - Connect to specific host (default: localhost)
- `--port PORT` - Connect to specific port (default: 8484)

## Data Model

### Agent Data

Each agent includes:
- `instanceId`: Unique agent identifier
- `name`: Agent name
- `hp`, `maxHp`: Current and maximum HP
- `mental`, `maxMental`: Current and maximum mental points
- `fortitude`, `prudence`, `temperance`, `justice`: Worker stats
- `currentSefira`: Currently assigned department
- `state`: Current state (IDLE, WORKING, PANIC, STUNNED, DEAD)
- `giftIds`: List of E.G.O. gift IDs equipped
- `weaponId`, `armorId`: Equipped E.G.O. weapon/suit
- `isDead`, `isPanicking`: Status flags

### Creature Data

Each abnormality includes:
- `instanceId`: Unique creature identifier
- `metadataId`: Creature metadata ID
- `name`: Abnormality name
- `riskLevel`: Risk level (ZAYIN, TETH, HE, WAW, ALEPH)
- `state`: Current state (IDLE, WORKING, ESCAPING, etc.)
- `qliphothCounter`, `maxQliphothCounter`: Qliphoth counter state
- `feelingState`: Current feeling state
- `currentSefira`: Assigned department
- `workCount`: Total work sessions completed
- `isEscaping`, `isSuppressed`: Status flags

### Game State

Overall game status includes:
- `day`: Current day number
- `gameState`: Game phase (STOP, PLAYING, etc.)
- `gameSpeed`: Current game speed (1-2)
- `energy`, `energyQuota`: Current energy and quota
- `managementStarted`: Whether management phase has started
- `isPaused`: Whether game is paused
- `emergencyLevel`: Current emergency level
- `playTime`: Total play time
- `lobPoints`: Current LOB points

### Sefira Data

Each department includes:
- `name`: Department name
- `sefiraEnum`: Enum value
- `isOpen`: Whether department is unlocked
- `openLevel`: Current open level
- `agentIds`: List of assigned agent IDs
- `creatureIds`: List of assigned creature IDs
- `officerCount`: Number of officers

## Protocol

Communication uses JSON-line protocol over TCP. Each message is a JSON object followed by a newline.

### Query Request

```json
{"id": "req-1", "type": "query", "target": "agents", "params": {"id": 3}}
```

### Response

```json
{"id": "req-1", "type": "response", "status": "ok", "data": {...}}
```

### Error Response

```json
{"id": "req-1", "type": "response", "status": "error", "error": "Agent not found", "code": "NOT_FOUND"}
```

## Future Phases

### Phase 2 - Event Streaming (Not Yet Implemented)

- Subscribe to game events (OnAgentDead, OnWorkStart, etc.)
- Wait for specific events with timeouts
- React to in-game happenings in real time

### Phase 3 - Commands (Not Yet Implemented)

- Debug commands (set stats, add gifts, fill energy, set qliphoth, spawn creature, etc.)
- Player-action simulation (assign work, pause/unpause, deploy/recall agents, suppress)

**Note:** The `shutdown` command is already implemented in Phase 1.5 for graceful game shutdown.

### Phase 4 - Mod Testing Workflows (Not Yet Implemented)

- Run scripted test scenarios
- Verify mod behavior in live game
- Visual state capture

## Troubleshooting

### Connection Refused

Ensure:
1. Lobotomy Corporation is running
2. LobotomyPlaywright plugin is installed and loaded
3. Port 8484 is not blocked by firewall

### No Query Results

The game may not be in a queryable state:
- On title screen: Not queryable
- Loading scenes: May be transiently unavailable
- Check `{baseDir}/scripts/query.py game` first for status

## References

- Game decompiled source: `external/decompiled/Assembly-CSharp/`
- Key managers: `GameManager`, `AgentManager`, `CreatureManager`, `SefiraManager`
- Event system: `Notice.cs`, `NoticeName.cs` (~80+ named events)
