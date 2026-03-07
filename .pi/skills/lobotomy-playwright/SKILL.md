---
name: lobotomy-playwright
description: Full observability and control bridge between a pi agent and a running Lobotomy Corporation game. Query game state, wait for events, issue commands, and verify outcomes programmatically.
---

# LobotomyPlaywright

LobotomyPlaywright is a Playwright-inspired system for observing and controlling a running Lobotomy Corporation game instance through a pi agent.

## Important Rules

1. **Always stop the game when finished.** When you are done with the game — whether your task succeeded, failed, or you encountered an error — you **must** run `dotnet playwright stop` before completing your response. Leaving the game running wastes system resources and blocks future launches.

## Prerequisites

The Lobotomy Corporation game must be running with the LobotomyPlaywright BepInEx plugin installed.

This skill uses a **dotnet local tool** (`playwright`) — no Python runtime required. All commands are invoked via `dotnet playwright <command>`.

### Initial Tool Installation

Install the local tool from the repo:

```bash
dotnet tool restore
```

This installs the `playwright` tool from the `LobotomyPlaywright/` project in this repository.

### macOS with CrossOver

1. CrossOver must be installed (for running the Windows game on macOS)
   - Download from https://www.codeweavers.com/crossover
   - Default path: `/Applications/CrossOver.app`

2. Game must be installed in a CrossOver bottle

3. BepInEx must be installed in the game directory

### Initial Setup

Run the auto-detection command to create configuration:

```bash
dotnet playwright find-game
```

This will:
- Scan CrossOver bottles for the game installation
- Validate BepInEx installation
- Create `config.json` with game path and settings

For manual configuration, use:

```bash
dotnet playwright find-game --path "/path/to/game" --bottle "BottleName"
```

## Usage

### Full Lifecycle Workflow

The complete build → deploy → launch → verify → stop workflow:

```bash
# First-time setup (auto-detect game path)
dotnet playwright find-game

# Deploy plugin to game
dotnet playwright deploy

# Launch game and wait for TCP readiness
dotnet playwright launch

# Check status
dotnet playwright status

# Query game state
dotnet playwright query game
dotnet playwright query agents

# Stop game when done
dotnet playwright stop
```

### Quick Redeploy

To rebuild, redeploy, and relaunch the game:

```bash
dotnet playwright stop
dotnet playwright deploy
dotnet playwright launch
dotnet playwright status
```

### Find Game (Auto-Detection)

```bash
dotnet playwright find-game                    # Auto-detect game path
dotnet playwright find-game --path "/path"     # Manual path specification
dotnet playwright find-game --check            # Validate existing config
dotnet playwright find-game --verbose          # Show detailed search output
```

### Deploy Plugin

```bash
dotnet playwright deploy                       # Build and deploy (Release)
dotnet playwright deploy --configuration Debug  # Build Debug config
dotnet playwright deploy --skip-build          # Deploy existing DLLs
dotnet playwright deploy --dry-run             # Show what would be done
```

The deploy command builds and copies the following DLLs:
- `LobotomyPlaywright.Plugin.dll` → `BepInEx/plugins/`
- `HarmonyDebugPanel.dll` → `BepInEx/plugins/`
- `RetargetHarmony.dll` → `BepInEx/patchers/`
- Harmony interop DLLs → `BepInEx/core/`

### Launch Game

```bash
dotnet playwright launch                       # Launch and wait for ready
dotnet playwright launch --no-wait            # Launch without waiting
dotnet playwright launch --force              # Relaunch (kill existing)
dotnet playwright launch --timeout 60         # Custom timeout (seconds)
```

### Check Status

```bash
dotnet playwright status                       # Current status (human-readable)
dotnet playwright status --json                # Status as JSON
dotnet playwright status --exit-code           # Exit non-zero if not READY
dotnet playwright status --wait-for ready     # Wait for READY status
dotnet playwright status --wait-for stopped    # Wait for STOPPED status
```

### Stop Game

```bash
dotnet playwright stop                         # Graceful shutdown, then force kill
dotnet playwright stop --force                 # Skip graceful shutdown
dotnet playwright stop --wait                  # Wait for processes to exit
```

### Read BepInX Logs

```bash
dotnet playwright read-log                      # Read main log file
dotnet playwright read-log --file LogOutput.log # Specify log file
dotnet playwright read-log --tail 50            # Show last 50 lines
dotnet playwright read-log --filter "Harmony"   # Filter lines containing "Harmony"
dotnet playwright read-log --list               # List available log files
```

Options:
- `--file FILE` - Specific log file to read (default: `LogOutput.log`)
- `--tail N` - Show only the last N lines
- `--filter TEXT` - Filter lines to only show those containing TEXT (case-insensitive)
- `--list` - List all available log files with sizes and timestamps

### Query Game State

```bash
dotnet playwright query agents                    # List all agents
dotnet playwright query agents <id>               # Get agent details
dotnet playwright query creatures                # List all abnormalities
dotnet playwright query creatures <id>            # Get creature details
dotnet playwright query game                     # Game state overview
dotnet playwright query departments               # Department status
```

### Query Parameters

- `--json` - Output raw JSON instead of formatted text
- `--host HOST` - Connect to specific host (default: localhost)
- `--port PORT` - Connect to specific port (default: 8484)

### Capture Screenshot

```bash
dotnet playwright screenshot                      # Capture screenshot (base64 by default)
dotnet playwright screenshot --format path       # Return only file path
dotnet playwright screenshot --format base64     # Return base64-encoded image data (default)
dotnet playwright screenshot --display text       # Display as formatted text (default)
dotnet playwright screenshot --display json       # Display as JSON
dotnet playwright screenshot --output image.png  # Save image to specific path
dotnet playwright screenshot --host localhost --port 8484  # Custom connection
```

**Format options:**
- `base64` - Returns base64-encoded PNG image data along with file info (default)
- `path` - Returns only the file path without the image data

**Display options:**
- `text` - Human-readable formatted output (default)
- `json` - Raw JSON output for programmatic parsing

**Output examples:**

Text output (default):
```
============================================================
Screenshot Captured
============================================================
  Filename: screenshot_20260306_180234_123.png
  Path: /path/to/game/LobotomyPlaywrightScreenshots/screenshot_20260306_180234_123.png
  Size: 1,234,567 bytes
  Timestamp (UTC): 2026-03-06T18:02:34.1234567Z
  Base64 (truncated): iVBORw0KGgoAAAANSUhEUgAA...

To decode and display the image:
  echo "iVBORw0KGgoAAAANSUhEUgAA..." | base64 -d > screenshot.png
  open screenshot.png  # macOS
============================================================
```

JSON output:
```json
{
  "status": "ok",
  "filename": "screenshot_20260306_180234_123.png",
  "path": "/path/to/game/.../screenshot_20260306_180234_123.png",
  "size": 1234567,
  "timestamp": "2026-03-06T18:02:34.1234567Z",
  "base64": "iVBORw0KGgoAAAANSUhEUgAA..."
}
```

**Notes:**
- Screenshots are saved to `<gamePath>/LobotomyPlaywrightScreenshots/`
- For the pi agent to display the image, use `--format base64 --display json` and decode the base64 data
- The `--output` option allows saving the image to any path; when combined with base64 format, the image is decoded and saved locally

### Wait for Events (Phase 2)

```bash
dotnet playwright wait event OnAgentDead --timeout 30
dotnet playwright wait event OnWorkStart OnWorkEnd --timeout 60
```

- Event names are from the game's Notice system (see `NoticeName.cs`)
- `--timeout` - Maximum seconds to wait (default: 30)
- Multiple events can be specified

### Issue Commands (Phase 3)

```bash
dotnet playwright command assign-work --agent 3 --creature 100001 --work instinct
dotnet playwright command pause
dotnet playwright command unpause
dotnet playwright command set-agent-stats --agent 3 --hp 100 --mental 100
dotnet playwright command fill-energy
```

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

### Phase 2 - Event Streaming (In Progress)

The plugin can stream game events. Wait for specific events:

```bash
dotnet playwright wait event <event-name> --timeout <seconds>
```

Available events are listed in `NoticeName.cs` (~80+ named events covering nearly every significant game action: agent death, work start/end, creature escape, ordeal activation, etc.)

### Phase 3 - Commands (In Progress)

The plugin can accept commands to manipulate game state and simulate player actions:

- **Debug commands** (direct state manipulation): `set-agent-stats`, `add-gift`, `remove-gift`, `set-qliphoth`, `fill-energy`, `set-game-speed`, `spawn-creature`, `trigger-ordeal`, `set-agent-invincible`
- **Player-action simulation** (what a player would click): `assign-work`, `pause`, `unpause`, `deploy-agent`, `recall-agent`, `suppress`

```bash
dotnet playwright command <action> [options]
```

**Note:** The `shutdown` command is already implemented in Phase 1.5 for graceful game shutdown.

### Phase 4 - Mod Testing Workflows (Not Yet Implemented)

- Run scripted test scenarios
- Verify mod behavior in live game
- Visual state capture

**Screenshot Capture (Implemented):**
- `dotnet playwright screenshot` - Capture the current game screen
- Returns base64-encoded image data for display by the pi agent
- Useful for visual verification of UI mods (e.g., GiftAlertIcon)

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
- Check `dotnet playwright query game` first for status

## References

- Game decompiled source: `external/decompiled/Assembly-CSharp/`
- Key managers: `GameManager`, `AgentManager`, `CreatureManager`, `SefiraManager`
- Event system: `Notice.cs`, `NoticeName.cs` (~80+ named events)
