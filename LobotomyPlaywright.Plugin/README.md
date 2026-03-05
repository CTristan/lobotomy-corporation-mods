# LobotomyPlaywright.Plugin

A BepInX 5 plugin for Lobotomy Corporation that provides a TCP server for querying and controlling game state remotely. Inspired by [Playwright](https://playwright.dev/), this enables pi agents to observe and interact with a running game instance programmatically.

## Features

### Phase 1 - Read-only Foundation (Implemented)

- **TCP Server**: Listens on `localhost:8484` (configurable via BepInEx config)
- **State Queries**:
  - List/query agents with full stats, equipment, and status
  - List/query abnormalities with qliphoth counters and observation info
  - Get game state (day, energy, phase, emergency level)
  - List departments with assigned agents and abnormalities
- **Unity Thread Safety**: All game state access happens on the main Unity thread via request queue

### Phase 2 - Event Streaming (Planned)

- Subscribe to game events via the Notice system
- Stream events to connected TCP clients
- Wait for specific events with timeouts

### Phase 3 - Commands (Planned)

- Debug commands (set stats, add gifts, fill energy, etc.)
- Player-action simulation (assign work, pause/unpause, suppression)

### Phase 4 - Mod Testing Workflows (Planned)

- Scripted test scenarios
- Automated mod verification
- Visual state capture

## Installation

1. Build the plugin:
   ```bash
   dotnet build LobotomyPlaywright.Plugin/LobotomyPlaywright.Plugin.csproj --configuration Release
   ```

2. Copy `LobotomyPlaywright.Plugin.dll` to your Lobotomy Corporation BepInX plugins folder:
   ```
   <GameDirectory>/BepInEx/plugins/LobotomyPlaywright.Plugin.dll
   ```

3. Launch Lobotomy Corporation

4. Check the BepInEx console for initialization message:
   ```
   [LobotomyPlaywright] LobotomyPlaywright v1.0.0 initialized.
   [LobotomyPlaywright] TCP server listening on 127.0.0.1:8484
   ```

## Configuration

Create `config/LobotomyPlaywright.Plugin.cfg` in your BepInX config directory:

```ini
[General]
Enabled = true
Port = 8484
```

- `Enabled`: Enable or disable the plugin (default: true)
- `Port`: TCP port for the server (default: 8484)

## Protocol

Communication uses JSON-line protocol over TCP. Each message is a JSON object followed by a newline (`\n`).

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

## Query Targets

### `agents`

- List all agents (no params)
- Get specific agent: `{"id": <instanceId>}`
- Returns: Agent data with stats, equipment, status

### `creatures`

- List all abnormalities (no params)
- Get specific creature: `{"id": <instanceId>}`
- Returns: Creature data with qliphoth counters, observation info

### `game`

- Get game state (no params)
- Returns: Day, phase, energy, emergency level, etc.

### `sefira` / `departments`

- List all departments (no params)
- Get specific department: `{"name": "MALKUTH"}` (or similar)
- Returns: Department status with assigned agents/creatures

## Architecture

```
Plugin.cs (Unity MonoBehaviour)
  ├── Awake(): Start TCP server
  ├── Update(): Process queued requests on main thread
  └── OnDestroy(): Stop TCP server

TcpServer (Background Thread)
  ├── Listen for connections
  └── Manage ClientHandler instances

ClientHandler (Per-connection, Background Thread)
  ├── Read JSON-line messages
  ├── Parse requests
  └── Enqueue for main-thread processing

RequestHandler (Main Thread)
  └── Route to QueryRouter/CommandRouter/EventSubscriptionManager

QueryRouter
  ├── AgentQueries
  ├── CreatureQueries
  ├── GameStateQueries
  └── SefiraQueries
```

## Thread Safety

Unity is single-threaded. The TCP server runs on a background thread, but all game state reads and writes must happen on the main Unity thread. The plugin uses a request queue pattern:

1. ClientHandler receives request (background thread)
2. Request enqueued for main-thread processing
3. Plugin.Update() processes queued requests
4. Response sent back on TCP thread

This avoids race conditions and Unity crashes.

## Development

### Prerequisites

- .NET Framework 3.5 SDK
- Lobotomy Corporation game files in `external/LobotomyCorp_Data/Managed/`

### Building

```bash
dotnet build LobotomyPlaywright.Plugin/LobotomyPlaywright.Plugin.csproj
```

### Testing

```bash
dotnet test LobotomyPlaywright.Plugin.Test/LobotomyPlaywright.Plugin.Test.csproj
```

### Code Style

This project follows the same conventions as the rest of the `lobotomy-corporation-mods` repository:

- File header: `// SPDX-License-Identifier: MIT`
- Allman brace style
- `var` preferred everywhere
- Private fields: `_camelCase`
- Null checks via guard clauses
- 100% code coverage target for testable logic

## Troubleshooting

### Plugin doesn't load

1. Check BepInEx console for errors
2. Verify `BepInEx/plugins/` directory exists
3. Ensure .NET 3.5 runtime is available

### Can't connect to TCP server

1. Check plugin is enabled in config
2. Verify port is not in use
3. Check firewall settings (localhost only)

### Game crashes on plugin load

1. Check game version compatibility (targeting original Lobotomy Corporation)
2. Verify all game DLL references are available in `external/`

## References

- Game decompiled source: `external/decompiled/Assembly-CSharp/`
- BepInX documentation: https://docs.bepinex.dev/
- Unity threading: https://docs.unity3d.com/Manual/ThreadSafety.html

## License

MIT
