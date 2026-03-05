---
name: lobotomy-playwright
description: Full observability and control bridge between a pi agent and a running Lobotomy Corporation game. Query game state, wait for events, issue commands, and verify outcomes programmatically.
---

# LobotomyPlaywright

LobotomyPlaywright is a Playwright-inspired system for observing and controlling a running Lobotomy Corporation game instance through a pi agent.

## Prerequisites

The Lobotomy Corporation game must be running with the LobotomyPlaywright BepInX plugin installed.

1. Build and copy `LobotomyPlaywright.Plugin.dll` to the game's BepInX plugins folder
2. Launch the game
3. The plugin will start a TCP server on `localhost:8484` (configurable via BepInEx config)

## Python Dependencies

```bash
pip install -r {baseDir}/requirements.txt
```

## Usage

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

- Debug commands (set stats, add gifts, fill energy, etc.)
- Player-action simulation (assign work, pause/unpause, suppress)

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
