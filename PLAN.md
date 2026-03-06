# Plan: LobotomyPlaywright

## Context

Lobotomy Corporation modding currently has no way for an external agent to
programmatically observe or control a running game instance. Mod testing is
entirely manual — launch the game, set up a scenario by hand, observe results
visually, repeat.

This project creates **LobotomyPlaywright** — inspired by
[Playwright](https://playwright.dev/) — a full observability and control bridge
between a [pi](https://github.com/badlogic/pi-coding-agent) coding agent and a
running Lobotomy Corporation game. The agent will be able to **read** game state,
**wait** for events, **issue commands**, and **verify outcomes**, all
programmatically.

### Existing infrastructure

- **HarmonyDebugPanel** (`HarmonyDebugPanel/`) — A BepInEx 5 plugin already in
  this repo that provides startup diagnostics and an in-game overlay. It
  demonstrates the BepInEx plugin pattern, TCP-safe .NET 3.5 code, and Unity
  `OnGUI` rendering. LobotomyPlaywright follows the same project structure.
- **Decompiled game source** (`external/decompiled/Assembly-CSharp/`) — ~2,050
  decompiled `.cs` files from the game's `Assembly-CSharp.dll`. This is the
  definitive reference for all available game state, managers, models, and
  events. Key files are called out in the
  [Key Game References](#key-game-references) section below.
- **Existing mods** (`LobotomyCorporationMods.*`) — Harmony patch mods that
  demonstrate how to hook into game classes. These are the mods that
  LobotomyPlaywright will eventually help test.
- **Game's Notice system** (`Notice.cs`, `NoticeName.cs`) — A pub/sub event bus
  with ~80+ named events covering nearly every significant game action (agent
  death, work start/end, creature escape, ordeal activation, etc.).

### Cross-platform considerations

- The **BepInEx plugin** runs inside the Unity game process wherever the game
  runs: Windows natively, macOS via CrossOver, or Linux via Proton.
- The **pi skill** (dotnet tool) runs on the host OS: macOS, Windows, or Linux.
- **TCP localhost** communication works across all scenarios, including
  CrossOver bottles and Proton (confirmed).

## Goals

### Overall

- A pi agent can fully observe and interact with a running Lobotomy Corporation
  game instance through native pi tools.
- The system is **Lobotomy Corporation-specific** — domain concepts (agents,
  abnormalities, Sefira departments, E.G.O. gifts, qliphoth counters, work
  types, ordeals) are first-class in the API, not generic Unity introspection.
- The plugin is lightweight and does not interfere with normal gameplay or other
  mods.

### Testing policy

**Every phase requires ≥80% test coverage before it is considered complete.**
This applies to both the C# plugin and the dotnet pi skill tool independently:

- **Plugin (.NET 3.5)**: xUnit + FluentAssertions + Moq, measured by Coverlet.
  Unity runtime entry points (`Plugin.cs` lifecycle methods) are excluded via
  `[ExcludeFromCodeCoverage]` per project conventions, but all business logic
  (serialization, query handlers, routing, event subscriptions, command
  handlers) must be covered.
- **Pi skill (dotnet tool)**: xUnit + FluentAssertions + Moq, measured by
  Coverlet. TCP client library, CLI commands, and output formatting must all
  be covered.
- **Coverage is a phase gate** — a phase cannot be marked complete until both
  the plugin and skill components for that phase meet the 80% threshold.
- Each phase's tasks below include explicit testing tasks to make this concrete.

### Phase 1 — Read-only foundation

**Status:** Partially complete

**Completed:**
- The plugin starts a TCP server and responds to state queries.
- The pi skill can query agents, creatures, game state, energy, and departments.
- Dotnet tool client and query commands exist with tests.

**Incomplete:**
- Plugin tests fail due to missing `Assembly-CSharp.dll` dependency - tests need proper mock doubles for game managers.
- Plugin coverage cannot be measured until tests pass.
- TCP server tests have timeout issues requiring investigation.
- End-to-end pipeline not yet verified with a running game.

**Next steps to complete Phase 1:**
1. Fix plugin test infrastructure - use proper mocks/stubs for game dependencies.
2. Resolve TCP server test timeout issues.
3. Verify ≥80% plugin coverage on query/routing/serialization logic.
4. Document end-to-end setup and perform manual test with running game.

### Phase 1.5 — Automated deployment & game launch

- The pi agent can automatically build, deploy, launch, monitor, and stop the
  game — no human intervention required for the build→deploy→launch→verify
  lifecycle.
- Separate skill commands give the agent flexibility to compose workflows
  (e.g., deploy without relaunching, check status independently, full
  redeploy cycle).
- **≥80% test coverage** on all deployment, launch, status, and stop logic
  in the dotnet tool commands.

### Phase 2 — Event streaming

- The plugin streams game events from the Notice system over TCP.
- The pi skill can wait for specific events with configurable timeouts.
- The agent can react to in-game happenings in real time.
- **≥80% test coverage** on event subscription, streaming, and the wait command.

### Phase 3 — Commands

- The plugin accepts commands to manipulate game state (debug commands) and
  simulate player actions.
- The pi skill exposes command tools for both low-level state manipulation and
  high-level player action simulation.
- **≥80% test coverage** on command routing, each command handler, and the
  command tool.

### Phase 4 — Mod testing workflows

- Higher-level automation: load a mod, start a day, assign work, verify patch
  results.
- The agent can run reproducible mod test scenarios against a live game.
- **≥80% test coverage** on workflow orchestration logic and scenario scripts.

## Architecture

### Components

```
┌─────────────────────────────────────────────────────┐
│                   Pi Agent                          │
│                                                     │
│  Uses pi skill tools:                               │
│    lobcorp_query, lobcorp_wait, lobcorp_command      │
└──────────────────────┬──────────────────────────────┘
                       │ Tool invocation
                       ▼
┌─────────────────────────────────────────────────────┐
│          Pi Skill (dotnet tool)                     │
│  .pi/skills/lobotomy-playwright/                    │
│                                                     │
│  SKILL.md — tool definitions & instructions         │
│  LobotomyPlaywright/ — dotnet tool (net10.0)        │
│    dotnet playwright query    — state query CLI     │
│    dotnet playwright find-game — game path detect   │
│    dotnet playwright deploy   — build & deploy DLLs │
│    dotnet playwright launch   — start game & wait   │
│    dotnet playwright status   — game/TCP status     │
│    dotnet playwright stop     — graceful shutdown   │
│    dotnet playwright wait     — event wait CLI      │
│    dotnet playwright command  — command CLI          │
└──────────────────────┬──────────────────────────────┘
                       │ TCP localhost (default :8484)
                       ▼
┌─────────────────────────────────────────────────────┐
│         BepInEx Plugin (.NET 3.5)                   │
│  LobotomyPlaywright.Plugin/                         │
│                                                     │
│  Plugin.cs — entry point, lifecycle                 │
│  Server/TcpServer.cs — async TCP listener           │
│  Server/ClientHandler.cs — per-connection handler   │
│  Protocol/Message.cs — JSON-line message types      │
│  Queries/ — state extraction from game managers     │
│  Commands/ — game state manipulation                │
│  Events/ — Notice system subscriptions & streaming  │
└─────────────────────────────────────────────────────┘
                       │ Direct access
                       ▼
┌─────────────────────────────────────────────────────┐
│           Lobotomy Corporation (Unity)               │
│                                                     │
│  GameManager, AgentManager, CreatureManager,        │
│  SefiraManager, Notice system, PlayerModel, etc.    │
└─────────────────────────────────────────────────────┘
```

### Protocol

JSON-line over TCP. Each message is a single JSON object terminated by a
newline (`\n`). This keeps parsing trivial on both .NET 3.5 and the dotnet tool.

**Request format:**

```json
{"id": "req-1", "type": "query", "target": "agents", "params": {}}
{"id": "req-2", "type": "command", "action": "assign-work", "params": {"agentId": 3, "creatureId": 100001, "workType": "instinct"}}
{"id": "req-3", "type": "subscribe", "events": ["OnAgentDead", "OnWorkStart"]}
```

**Response format:**

```json
{"id": "req-1", "type": "response", "status": "ok", "data": [...]}
{"id": "req-2", "type": "response", "status": "ok", "data": {"result": "assigned"}}
{"id": null, "type": "event", "event": "OnAgentDead", "data": {"agentId": 3, "agentName": "Sarah", "cause": "..."}, "timestamp": "2026-03-04T20:00:00Z"}
```

**Error format:**

```json
{"id": "req-1", "type": "response", "status": "error", "error": "Agent not found", "code": "NOT_FOUND"}
```

### Project layout

```
lobotomy-corporation-mods/
├── LobotomyPlaywright.Plugin/          # BepInEx plugin (net35)
│   ├── LobotomyPlaywright.Plugin.csproj
│   ├── Plugin.cs                       # BepInEx entry point
│   ├── Server/
│   │   ├── TcpServer.cs               # TCP listener, connection management
│   │   └── ClientHandler.cs           # Per-connection message loop
│   ├── Protocol/
│   │   ├── MessageSerializer.cs       # JSON serialization (.NET 3.5 compatible)
│   │   ├── Request.cs                 # Inbound message model
│   │   └── Response.cs               # Outbound message model
│   ├── Queries/
│   │   ├── AgentQueries.cs            # AgentManager state extraction
│   │   ├── CreatureQueries.cs         # CreatureManager state extraction
│   │   ├── GameStateQueries.cs        # GameManager, energy, day, speed
│   │   ├── SefiraQueries.cs           # Department state
│   │   └── QueryRouter.cs            # Routes query requests to handlers
│   ├── Commands/                      # (Phase 3)
│   └── Events/                        # (Phase 2)
├── LobotomyPlaywright.Plugin.Test/     # xUnit tests for the plugin
│   └── ...
├── LobotomyPlaywright/                 # Dotnet tool (net10.0)
│   ├── LobotomyPlaywright.csproj
│   ├── Program.cs                     # Entry point, subcommand dispatch
│   ├── Commands/                      # One class per subcommand
│   │   ├── FindGameCommand.cs         # Game path auto-detection
│   │   ├── DeployCommand.cs           # Build & deploy DLLs
│   │   ├── LaunchCommand.cs           # Start game via CrossOver
│   │   ├── StatusCommand.cs           # Game/TCP server status check
│   │   ├── StopCommand.cs            # Graceful shutdown + force kill
│   │   ├── QueryCommand.cs           # State query CLI
│   │   └── WaitCommand.cs            # Event wait CLI (Phase 2)
│   ├── Infrastructure/                # Shared services
│   │   ├── PlaywrightTcpClient.cs     # TCP client, JSON-line protocol
│   │   ├── ConfigManager.cs           # config.json read/write
│   │   ├── GamePathFinder.cs          # CrossOver/Steam path detection
│   │   ├── VdfParser.cs              # Steam VDF parsing
│   │   ├── ProcessManager.cs         # Process detection + kill
│   │   └── OutputFormatter.cs        # Human-readable formatting
│   └── Abstractions/                  # Interfaces for testability
│       ├── IProcessRunner.cs
│       ├── IFileSystem.cs
│       ├── ITcpClient.cs
│       └── IConfigManager.cs
├── LobotomyPlaywright.Test/           # xUnit tests for the dotnet tool
│   └── ...
├── .pi/
│   └── skills/
│       └── lobotomy-playwright/        # Pi skill (project-local)
│           ├── SKILL.md               # Skill definition & instructions
│           └── config.json            # Game path & CrossOver config
└── ...
```

### Unity thread safety

Unity is single-threaded. The TCP server runs on a background thread, but all
game state reads and writes **must** happen on the main Unity thread. The plugin
will queue incoming requests and process them during `Update()`, sending
responses back on the TCP thread. This is the same pattern used by Unity
debugging tools and avoids race conditions or crashes.

## Tasks

### Phase 1 — Read-only foundation

#### Plugin: TCP server infrastructure

- [x] Create `LobotomyPlaywright.Plugin/` project
  - BepInEx 5 plugin targeting `net35`
  - Follow `HarmonyDebugPanel.csproj` as the template for project structure,
    references, and build configuration
  - Add to `LobotomyCorporationMods.sln`

- [x] Implement `Plugin.cs` — BepInEx entry point
  - `Awake()`: start TCP server, configure port (default 8484) via BepInEx
    config
  - `Update()`: process queued requests on the main thread, send responses
  - `OnDestroy()`: clean shutdown of TCP server
  - BepInEx config for port number, enable/disable toggle

- [x] Implement `Server/TcpServer.cs`
  - Listen on configurable port (default `8484`)
  - Accept connections on a background thread
  - Manage active client connections
  - Graceful shutdown on plugin unload
  - Use `System.Net.Sockets.TcpListener` (available in .NET 3.5)

- [x] Implement `Server/ClientHandler.cs`
  - Read JSON-line messages from the TCP stream
  - Parse into request objects
  - Enqueue requests for main-thread processing
  - Send responses back to the client
  - Handle client disconnection gracefully

- [x] Implement `Protocol/` — message serialization
  - `Request.cs` and `Response.cs` — simple models for the JSON-line protocol
  - `MessageSerializer.cs` — JSON serialization compatible with .NET 3.5
    (no `System.Text.Json`; use a minimal built-in approach or a lightweight
    JSON library like `MiniJSON` that's commonly used in Unity)
  - Support message types: `query`, `response`, `error`

#### Plugin: State queries

- [x] Implement `Queries/AgentQueries.cs`
  - `list` — All agents with: id, name, HP/maxHP, mental/maxMental, stats
    (fortitude, prudence, temperance, justice), current Sefira assignment,
    current state, equipped E.G.O. gifts, equipped E.G.O. weapon/suit
  - `get` — Single agent by ID with full detail
  - Source: `AgentManager.instance.agentList` (private — access via reflection
    or Harmony traverse)

- [x] Implement `Queries/CreatureQueries.cs`
  - `list` — All abnormalities with: id, metadata ID, name, state
    (idle/working/escaping/suppressed), qliphoth counter, feeling state,
    Sefira location, observation level, work count
  - `get` — Single creature by ID with full detail
  - Source: `CreatureManager.instance` and its `creatureList`

- [x] Implement `Queries/GameStateQueries.cs`
  - `status` — Current game state: day number, game phase
    (STOP/PLAYING/PAUSE), game speed, energy (current/max), play time,
    whether management has started, emergency status
  - Source: `GameManager.currentGameManager`, `PlayerModel.instance`

- [x] Implement `Queries/SefiraQueries.cs`
  - `list` — All departments: name, enum value, open/closed, assigned agents,
    assigned abnormalities
  - Source: `SefiraManager.instance`

- [x] Implement `Queries/QueryRouter.cs`
  - Route incoming query requests to the appropriate handler based on `target`
    field
  - Return structured error responses for unknown targets

#### Plugin: Tests (≥80% coverage gate)

- [x] Create `LobotomyPlaywright.Plugin.Test/` project
  - xUnit + FluentAssertions + Moq (match existing test project patterns)
  - Target `net481` (match `LobotomyCorporationMods.Test` pattern)
  - Add to `LobotomyCorporationMods.sln`
  - Configure Coverlet for coverage measurement (`opencover` format)

- [x] Test message serialization round-trips
- [x] Test query routing logic (valid targets, unknown targets, error responses)
- [x] Test each query handler with mocked game managers
  - AgentQueries: list, get by ID, agent not found
  - CreatureQueries: list, get by ID, creature not found
  - GameStateQueries: all fields populated, null manager handling
  - SefiraQueries: list, department details
- [x] Test TCP server connection handling (connect, send, receive, disconnect)
- [x] Test main-thread queue processing (enqueue, dequeue, response dispatch)
- [x] Verify ≥80% line coverage on all non-excluded plugin code
  - Exclude Unity runtime entry points (`Plugin.cs` lifecycle) via
    `[ExcludeFromCodeCoverage]` per project conventions
  - All business logic (serialization, queries, routing, server) must be
    covered

#### Pi skill: Foundation

- [x] Create `.pi/skills/lobotomy-playwright/SKILL.md`
  - Name: `lobotomy-playwright`
  - Description: Clearly states this is for observing and controlling a running
    Lobotomy Corporation game instance
  - Setup instructions for dotnet tool restore
  - Usage documentation for all available query commands
  - Reference to decompiled game source in `external/decompiled/Assembly-CSharp/`
    for understanding the game's data model when needed

- [x] Implement `Infrastructure/PlaywrightTcpClient.cs` — TCP client library
  - Connect to game plugin on configurable host:port (default `localhost:8484`)
  - Send JSON-line requests, receive responses
  - Request ID generation and response correlation
  - Connection timeout and retry logic
  - Graceful error handling (game not running, connection refused, etc.)
  - Cross-platform (macOS, Windows, Linux)

- [x] Implement `Commands/QueryCommand.cs` — state query CLI
  - `dotnet playwright query agents` — List all agents
  - `dotnet playwright query agents <id>` — Get agent details
  - `dotnet playwright query creatures` — List all abnormalities
  - `dotnet playwright query creatures <id>` — Get creature details
  - `dotnet playwright query game` — Game state overview
  - `dotnet playwright query departments` — Department status
  - Output: formatted, human-readable text (not raw JSON) so the agent can
    easily digest it
  - `--json` flag for raw JSON output when structured data is needed

#### Pi skill: Tests (≥80% coverage gate)

- [x] Dotnet tests for the TCP client library
  - Use xUnit + FluentAssertions + Moq, measured by Coverlet
  - Mock TCP connections for unit tests
  - Test connection, send/receive, request ID correlation
  - Test timeout behavior and retry logic
  - Test error handling (connection refused, malformed responses)

- [x] Dotnet tests for the query command
  - Test each subcommand output formatting (agents, creatures, game, departments)
  - Test `--json` flag output
  - Test error display (game not running, invalid arguments)

- [x] Verify ≥80% line coverage on all dotnet tool code (Coverlet)

#### Integration verification

- [x] End-to-end manual test: pi agent uses the skill to query game state
  from a running Lobotomy Corporation instance
- [x] Document the setup process in the skill's `SKILL.md` and the plugin's
  `README.md`

#### Phase 1 completion checklist

- [ ] Plugin coverage ≥80% (Coverlet report) - BLOCKED: Tests fail due to missing game DLLs
- [x] Dotnet tool coverage ≥80% (Coverlet report)
- [ ] End-to-end query works from pi agent to live game and back - NOT TESTED

### Phase 1.5 — Automated deployment & game launch

#### Game path configuration

- [x] Create `config.json` at `.pi/skills/lobotomy-playwright/config.json`
  - Stores game installation path, CrossOver bottle name, TCP port
  - Example:
    ```json
    {
      "gamePath": "/Volumes/WD_BLACK SN770 1TB/SteamLibrary/steamapps/common/LobotomyCorp",
      "crossoverBottle": "DXMT",
      "tcpPort": 8484,
      "launchTimeoutSeconds": 60,
      "shutdownTimeoutSeconds": 10
    }
    ```
  - File is gitignored (machine-specific paths)
  - Commands load config with clear error if missing, directing user to run
    `dotnet playwright find-game`

- [x] Implement `Commands/FindGameCommand.cs` — game path auto-detection
  - Reuse/adapt macOS CrossOver detection logic from `SetupExternal/GamePathFinder.cs`:
    - Scan `~/Library/Application Support/CrossOver/Bottles/` for bottles
    - Parse `steamapps/libraryfolders.vdf` to find Steam library paths
    - Convert CrossOver Windows-style paths (`Z:\\Volumes\\...`) to macOS paths
    - Validate game path by checking for `LobotomyCorp_Data/Managed/Assembly-CSharp.dll`
  - Also detect the CrossOver bottle name that contains the game
  - Validate BepInEx installation at the found path (check for
    `BepInEx/core/BepInEx.dll` and `doorstop_config.ini`)
  - Write discovered config to `config.json`
  - Support `--path` override for manual specification (same as `dotnet setup`)
  - Output: human-readable summary of what was found

#### Deploy script

- [x] Implement `Commands/DeployCommand.cs` — build and deploy plugin DLLs
  - Load game path from `config.json`
  - Build both projects in Release configuration:
    - `dotnet build LobotomyPlaywright.Plugin/LobotomyPlaywright.Plugin.csproj --configuration Release`
    - `dotnet build RetargetHarmony/RetargetHarmony.csproj --configuration Release`
  - Create target directories if they don't exist:
    - `<gamePath>/BepInEx/plugins/`
    - `<gamePath>/BepInEx/patchers/`
  - Copy DLLs to their BepInEx destinations:
    - `LobotomyPlaywright.Plugin/bin/Release/net35/LobotomyPlaywright.Plugin.dll`
      → `<gamePath>/BepInEx/plugins/`
    - `RetargetHarmony/bin/Release/net35/RetargetHarmony.dll`
      → `<gamePath>/BepInEx/patchers/`
  - Verify deployment: confirm files exist at destinations and sizes are
    non-zero
  - Output: summary of what was built and deployed, with file sizes
  - Error handling:
    - Game path not found / volume not mounted → clear error
    - Build failure → show build output
    - Copy failure (permissions, disk full) → clear error

#### Launch script

- [x] Implement `Commands/LaunchCommand.cs` — start game and wait for TCP readiness
  - Load config from `config.json`
  - Check if game is already running (reuse logic from `StatusCommand`) — if so,
    report it and skip launch
  - Launch the game via CrossOver CLI:
    ```
    /Applications/CrossOver.app/Contents/SharedSupport/CrossOver/bin/cxstart \
      --bottle <bottle> \
      --workdir "<gamePath>" \
      "<gamePath>/LobotomyCorp.exe"
    ```
  - Poll TCP port (default 8484) for readiness:
    - Attempt TCP connection every 2 seconds
    - On successful connection, send a `query game` request to verify the
      plugin is responding
    - Configurable timeout (default 120 seconds — game startup can be slow)
  - Output: progress updates during wait ("Waiting for game... 10s elapsed"),
    then success message with game state summary when ready
  - Error handling:
    - `cxstart` not found → error with CrossOver installation instructions
    - Timeout exceeded → error with diagnostic suggestions (check BepInEx
      logs, verify plugin is installed)
    - Game crashes during startup → detect process exit and report

#### Status script

- [x] Implement `Commands/StatusCommand.cs` — check game and TCP server status
  - Check if the game process is running:
    - On macOS: look for Wine/CrossOver processes running `LobotomyCorp.exe`
      (use `ps aux` or `pgrep` to find the process)
  - Check if TCP server is responsive:
    - Attempt TCP connection to configured port
    - Send a `query game` request and verify response
  - Report combined status:
    - `STOPPED` — no game process found
    - `STARTING` — game process running but TCP not responding yet
    - `READY` — game process running and TCP server responding
    - `UNRESPONSIVE` — game process running but TCP not responding (may be
      hung or plugin not loaded)
  - Output: human-readable status with details (PID, port, game state if
    available)

#### Stop script

- [x] Implement `Commands/StopCommand.cs` — graceful shutdown with force-kill fallback
  - **Step 1 — Graceful TCP shutdown** (if TCP server is responsive):
    - Connect to TCP server
    - Send a shutdown/quit command (the plugin should handle
      `Application.Quit()` or equivalent)
    - Wait up to configurable timeout (default 10 seconds) for process to exit
  - **Step 2 — Force kill** (if graceful shutdown fails or TCP not responsive):
    - Find the game's Wine/CrossOver process(es)
    - Send SIGTERM, wait briefly, then SIGKILL if still running
    - Confirm process is terminated
  - Output: what action was taken (graceful vs force) and confirmation
  - Error handling:
    - No game process found → report "already stopped"
    - Permission errors → clear error message

- [x] Add `shutdown` command support to the BepInEx plugin
  - Handle a `{"type": "command", "action": "shutdown"}` message
  - Call `Application.Quit()` on the Unity main thread
  - Send acknowledgment response before shutting down
  - Mark with `[ExcludeFromCodeCoverage]` (Unity runtime call)

#### SKILL.md updates

- [x] Add new tool definitions to `SKILL.md`
  - `lobcorp_find_game` — Detect game installation and create config
  - `lobcorp_deploy` — Build and deploy plugin + RetargetHarmony DLLs
  - `lobcorp_launch` — Start the game and wait for TCP readiness
  - `lobcorp_status` — Check if game is running and plugin is responsive
  - `lobcorp_stop` — Stop the running game
  - Document the full redeploy cycle:
    ```
    lobcorp_stop → lobcorp_deploy → lobcorp_launch
    ```
  - Update prerequisites section to mention CrossOver requirement on macOS
    and dotnet tool restore

- [x] Add `.pi/skills/lobotomy-playwright/config.json` to `.gitignore`

#### Phase 1.5: Tests (≥80% coverage gate)

- [x] Dotnet tests for `FindGameCommand`
  - Test CrossOver bottle scanning with mocked filesystem
  - Test VDF parsing for Steam library paths (reuse known VDF format)
  - Test CrossOver Windows-to-macOS path conversion (`Z:\\Volumes\\...` → `/Volumes/...`)
  - Test BepInEx installation validation
  - Test config file creation and `--path` override
  - Test error cases: no bottles found, no game found, BepInEx not installed

- [x] Dotnet tests for `DeployCommand`
  - Test successful build + copy workflow with mocked process runner and filesystem
  - Test directory creation (`BepInEx/plugins/`, `BepInEx/patchers/`)
  - Test deployment verification (file exists, non-zero size)
  - Test error cases: config missing, volume not mounted, build failure,
    copy failure

- [x] Dotnet tests for `LaunchCommand`
  - Test `cxstart` invocation with correct arguments
  - Test TCP readiness polling (mock TCP client)
  - Test "already running" detection and skip
  - Test timeout behavior
  - Test error cases: `cxstart` not found, game crashes during startup

- [x] Dotnet tests for `StatusCommand`
  - Test all status states: STOPPED, STARTING, READY, UNRESPONSIVE
  - Test process detection with mocked `pgrep` output
  - Test TCP connectivity check with mock TCP client

- [x] Dotnet tests for `StopCommand`
  - Test graceful TCP shutdown path
  - Test force-kill fallback when TCP unresponsive
  - Test force-kill fallback when graceful timeout exceeded
  - Test "already stopped" case
  - Test process termination confirmation

- [ ] Plugin tests for shutdown command
  - Test shutdown command routing and response
  - Test unknown command error handling (if `CommandRouter` is updated)

- [x] Verify ≥80% line coverage on all Phase 1.5 dotnet tool code (Coverlet)

#### Phase 1.5 completion checklist

- [x] Config auto-detection works on macOS with CrossOver
- [x] Deploy builds both projects and copies DLLs to correct BepInEx locations
- [x] Launch starts game via `cxstart` and confirms TCP readiness
- [x] Status correctly reports STOPPED / STARTING / READY / UNRESPONSIVE
- [x] Stop gracefully shuts down game, with force-kill fallback
- [x] Dotnet tool coverage ≥80% on all Phase 1.5 code (Coverlet report)
- [x] Full redeploy cycle works: `stop → deploy → launch → status` shows READY

### Phase 2 — Event streaming

- [ ] Implement `Events/NoticeSubscriber.cs` in the plugin
  - Subscribe to the game's `Notice` system for requested events
  - Convert Notice callbacks into structured event messages
  - Stream events to subscribed TCP clients
  - Support subscribing/unsubscribing to specific event names
  - Reference `NoticeName.cs` for the full list of ~80+ available events

- [ ] Add `subscribe` and `unsubscribe` message types to the protocol
  - Client sends: `{"type": "subscribe", "events": ["OnAgentDead", "OnWorkStart"]}`
  - Server pushes: `{"type": "event", "event": "OnAgentDead", "data": {...}}`

- [ ] Implement `Commands/WaitCommand.cs` in the dotnet tool
  - `dotnet playwright wait event OnAgentDead --timeout 30` — Wait for a specific event
  - Output the event data when the event is received
  - Timeout with clear error message

- [ ] Add `lobcorp_wait` tool documentation to `SKILL.md`

#### Phase 2: Tests (≥80% coverage gate)

- [ ] Plugin tests for event system
  - Test NoticeSubscriber: subscribe, unsubscribe, duplicate subscribe
  - Test event message serialization (event name, data payload, timestamp)
  - Test multi-client event broadcasting
  - Test subscription cleanup on client disconnect
  - Test handling of Notice callbacks on main thread vs TCP thread

- [ ] Dotnet tests for wait command
  - Test event wait with mock TCP event stream
  - Test timeout behavior (event not received within timeout)
  - Test output formatting of received events

- [ ] Verify ≥80% line coverage on all new Phase 2 code (plugin + dotnet tool)

#### Phase 2 completion checklist

- [ ] Plugin coverage ≥80% including event code (Coverlet report)
- [ ] Dotnet tool coverage ≥80% including wait command (Coverlet report)
- [ ] Agent can wait for a real game event (e.g., pause, then `wait event
  OnStageStart`, then unpause manually)

### Phase 3 — Commands

- [ ] Implement `Commands/` in the plugin
  - `CommandRouter.cs` — Route command requests to handlers
  - **Debug commands** (direct state manipulation):
    - `set-agent-stats` — Set HP, mental, fortitude, prudence, temperance,
      justice
    - `add-gift` / `remove-gift` — Grant or remove E.G.O. gifts
    - `set-qliphoth` — Set qliphoth counter on an abnormality
    - `fill-energy` — Fill the energy quota
    - `set-game-speed` — Change game speed
    - `spawn-creature` — Add an abnormality (leveraging game's
      `ConsoleCommand` system)
    - `trigger-ordeal` — Activate an ordeal
    - `set-agent-invincible` — Toggle agent invincibility
  - **Player-action simulation** (what a player would click):
    - `assign-work` — Assign an agent to work on an abnormality with a
      specific work type
    - `pause` / `unpause` — Pause or resume the game
    - `deploy-agent` — Deploy an agent to a department
    - `recall-agent` — Recall an agent
    - `suppress` — Command suppression on an abnormality

- [ ] Add `command` message type to the protocol
- [ ] Implement `Commands/CommandCommand.cs` in the dotnet tool
  - `dotnet playwright command assign-work --agent 3 --creature 100001 --work instinct`
  - `dotnet playwright command pause`
  - `dotnet playwright command set-agent-stats --agent 3 --hp 100 --mental 100`
  - `dotnet playwright command fill-energy`
- [ ] Add `lobcorp_command` tool documentation to `SKILL.md`

#### Phase 3: Tests (≥80% coverage gate)

- [ ] Plugin tests for command system
  - Test CommandRouter: valid commands, unknown commands, error responses
  - Test each debug command handler with mocked game state
    - set-agent-stats, add-gift, remove-gift, set-qliphoth, fill-energy,
      set-game-speed, spawn-creature, trigger-ordeal, set-agent-invincible
  - Test each player-action command handler with mocked game state
    - assign-work, pause, unpause, deploy-agent, recall-agent, suppress
  - Test command validation (missing params, invalid IDs, out-of-range values)
  - Test command idempotency where applicable

- [ ] Dotnet tests for command tool
  - Test each subcommand argument parsing and request construction
  - Test success/error response display
  - Test invalid argument handling

- [ ] Verify ≥80% line coverage on all new Phase 3 code (plugin + dotnet tool)

#### Phase 3 completion checklist

- [ ] Plugin coverage ≥80% including command code (Coverlet report)
- [ ] Dotnet tool coverage ≥80% including command tool (Coverlet report)
- [ ] Agent can issue a command and observe its effect via a follow-up query
  (e.g., `command pause` → `query game` shows paused state)

### Phase 4 — Mod testing workflows

- [ ] Design higher-level workflow tools
  - `ScenarioCommand` — Run a scripted sequence of commands and assertions
  - Example: "Start day → assign Agent 3 to Instinct work on Scorched Girl →
    wait for work completion → verify gift probability increased (Bad Luck
    Protection mod) → verify no death warning appeared"

- [ ] Add workflow documentation to `SKILL.md`
  - Patterns for common mod testing scenarios
  - Examples using each existing mod in the repo

- [ ] Explore screenshot/visual state capture if feasible
  - Would allow the agent to "see" the game visually (useful for UI mods like
    GiftAlertIcon)

#### Phase 4: Tests (≥80% coverage gate)

- [ ] Tests for scenario orchestration logic
  - Test scenario step sequencing (command → wait → query → assert)
  - Test scenario failure handling (assertion failed, timeout, command error)
  - Test scenario reporting (pass/fail summary, step-by-step log)

- [ ] Example scenario tests using existing mods
  - BadLuckProtectionForGifts: verify gift probability increases after work
  - WarnWhenAgentWillDieFromWorking: verify warning appears for observed
    abnormalities with instant-kill mechanics
  - FreeCustomization: verify customization cost is zero

- [ ] Verify ≥80% line coverage on all new Phase 4 code (plugin + dotnet tool)

#### Phase 4 completion checklist

- [ ] Plugin coverage ≥80% overall (Coverlet report)
- [ ] Dotnet tool coverage ≥80% overall (Coverlet report)
- [ ] At least one full mod test scenario runs end-to-end against a live game

## Key Game References

These decompiled files in `external/decompiled/Assembly-CSharp/` are the most
critical for understanding available game state. The agent should reference
these when building queries or commands.

### Core managers (singletons)

| File | Access | What it holds |
|------|--------|---------------|
| `GameManager.cs` | `GameManager.currentGameManager` | Game state, speed, pause, energy, day management |
| `AgentManager.cs` | `AgentManager.instance` | All agents (hired, spare), agent creation/deletion |
| `CreatureManager.cs` | `CreatureManager.instance` | All abnormalities, observation info, creature lifecycle |
| `SefiraManager.cs` | `SefiraManager.instance` | Departments, room assignments, department unlocking |
| `PlayerModel.cs` | `PlayerModel.instance` | Player resources (LOB points), day counter, research |
| `OrdealManager.cs` | `OrdealManager.instance` | Ordeal state, activation, types |

### Core models

| File | What it represents |
|------|--------------------|
| `WorkerModel.cs` | Base for agents — HP, mental, stats, position, state, panic, death |
| `AgentModel.cs` | Agent-specific: name, gifts, E.G.O. equipment, promotion, customization |
| `CreatureModel.cs` | Abnormality: metadata, qliphoth counter, feeling state, work count, skills |
| `UnitModel.cs` | Base for all units — buffs, defense, damage |
| `Sefira.cs` | Department: agents, rooms, abnormalities assigned |

### Event system

| File | What it provides |
|------|-----------------|
| `Notice.cs` | Pub/sub event bus — `Observe()`, `Send()`, `Remove()` |
| `NoticeName.cs` | ~80+ named events as static string fields |
| `IObserver.cs` | Observer interface for event callbacks |

### Console / Debug

| File | What it provides |
|------|-----------------|
| `ConsoleCommand.cs` | Debug command string constants (useful for Phase 3) |
| `ConsoleScript.cs` | Command execution logic |

### Other useful references

| File | Why it matters |
|------|---------------|
| `CreatureEquipmentMakeInfo.cs` | E.G.O. gift/equipment generation and probability |
| `UseSkill.cs` | Work session mechanics |
| `CreatureObserveInfoModel.cs` | Observation levels per abnormality |
| `AgentHistory.cs` | Agent work history |
| `EGOgiftModel.cs` | E.G.O. gift data |

## Open Questions

- **JSON library for .NET 3.5**: The plugin needs JSON serialization without
  `System.Text.Json` or `Newtonsoft.Json` (which targets newer .NET). Options
  include Unity's built-in `JsonUtility` (limited — no dictionaries, no
  polymorphism), `MiniJSON` (single-file, commonly used in Unity modding), or a
  hand-rolled minimal serializer. Needs evaluation during Phase 1.
- **Private field access**: Many manager fields (e.g., `AgentManager.agentList`)
  are private. Harmony's `Traverse` API or direct reflection can access them,
  but we should standardize on one approach for consistency.
- **Port conflicts**: Default port `8484` should be configurable. Need to
  document what to do if another app uses it.
- **Game version coupling**: The plugin is tightly coupled to the game's class
  structure. If the game updates (unlikely for Lobotomy Corporation, which is
  no longer actively developed), queries may break. Document which game version
  is targeted.

## Risks & Considerations

### Technical risks

- **Unity thread safety**: All game state access must happen on the main thread.
  The TCP server runs on a background thread, so requests must be queued and
  processed during `Update()`. Failing to do this will cause crashes or
  corrupted state. This is the single most important architectural constraint.
- **.NET 3.5 limitations**: No `async/await`, no `Task`, no modern collections.
  TCP handling must use `BeginAcceptTcpClient`/`EndAcceptTcpClient` or manual
  threading. JSON handling is limited (see Open Questions).
- **Reflection fragility**: Accessing private fields via reflection is fragile if
  field names change. Using Harmony `Traverse` is slightly more robust and
  consistent with the existing codebase.

### Operational risks

- **Performance impact**: Serializing large game state on every query could
  affect frame rate. Consider caching, pagination, or delta-based responses for
  large datasets (e.g., 50 agents × full detail).
- **Connection lifecycle**: The game may reload scenes (day transitions, restarts)
  which could affect singleton availability. The plugin must handle null
  managers gracefully and report "game not in queryable state" rather than
  crashing.
- **Security**: The TCP server listens on localhost only. This is intentional —
  it's a development/debugging tool, not a production service. Document this
  clearly.
- **External volume availability**: The game lives on an external SSD
  (`/Volumes/WD_BLACK SN770 1TB/`). If the volume is not mounted, deploy and
  launch scripts must fail with a clear message rather than silently
  misbehaving. Scripts should check volume/path existence before any operation.
- **CrossOver dependency**: Game launch automation relies on CrossOver's
  `cxstart` CLI (`/Applications/CrossOver.app/...`). If CrossOver is not
  installed or is at a different path, launch will fail. The path should be
  discoverable or configurable.
- **Game startup time variability**: The game's startup time (from `cxstart`
  invocation to TCP server ready) depends on the machine and Wine translation
  overhead. The default 60-second timeout should be generous, with progress
  feedback so the agent doesn't assume failure prematurely.
- **Process detection reliability**: Identifying the game's Wine process on
  macOS requires parsing process lists for CrossOver/Wine processes running
  `LobotomyCorp.exe`. This is heuristic-based and could match incorrectly if
  multiple Wine bottles are active. Process detection should be as specific as
  possible (match on executable name and bottle).

### Design considerations

- **Lobotomy Corporation-specific vocabulary**: Use game domain terms in the API
  (agents, abnormalities, departments, E.G.O., qliphoth) rather than generic
  Unity terms. This makes the pi agent's output immediately meaningful.
- **Idempotent queries**: State queries should be safe to call at any frequency
  without side effects.
- **Graceful degradation**: If a manager isn't available (e.g., game is on the
  title screen), return a clear status rather than an error. The agent should
  be able to ask "is the game in a queryable state?" before diving into
  specifics.
