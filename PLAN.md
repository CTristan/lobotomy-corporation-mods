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
- The **pi skill** (Python) runs on the host OS: macOS, Windows, or Linux.
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

### Phase 1 — Read-only foundation

- The plugin starts a TCP server and responds to state queries.
- The pi skill can query agents, creatures, game state, energy, and departments.
- End-to-end pipeline is proven: pi agent → skill → Python client → TCP →
  plugin → game state → response.

### Phase 2 — Event streaming

- The plugin streams game events from the Notice system over TCP.
- The pi skill can wait for specific events with configurable timeouts.
- The agent can react to in-game happenings in real time.

### Phase 3 — Commands

- The plugin accepts commands to manipulate game state (debug commands) and
  simulate player actions.
- The pi skill exposes command tools for both low-level state manipulation and
  high-level player action simulation.

### Phase 4 — Mod testing workflows

- Higher-level automation: load a mod, start a day, assign work, verify patch
  results.
- The agent can run reproducible mod test scenarios against a live game.

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
│              Pi Skill (Python)                      │
│  .pi/skills/lobotomy-playwright/                    │
│                                                     │
│  SKILL.md — tool definitions & instructions         │
│  scripts/client.py — TCP client, JSON-line protocol │
│  scripts/query.py  — state query CLI                │
│  scripts/wait.py   — event wait CLI                 │
│  scripts/command.py — command CLI                   │
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
newline (`\n`). This keeps parsing trivial on both .NET 3.5 and Python.

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
├── .pi/
│   └── skills/
│       └── lobotomy-playwright/        # Pi skill (project-local)
│           ├── SKILL.md               # Skill definition & instructions
│           └── scripts/
│               ├── client.py          # TCP client library
│               ├── query.py           # State query CLI
│               ├── wait.py            # Event wait CLI (Phase 2)
│               └── command.py         # Command CLI (Phase 3)
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

- [ ] Create `LobotomyPlaywright.Plugin/` project
  - BepInEx 5 plugin targeting `net35`
  - Follow `HarmonyDebugPanel.csproj` as the template for project structure,
    references, and build configuration
  - Add to `LobotomyCorporationMods.sln`

- [ ] Implement `Plugin.cs` — BepInEx entry point
  - `Awake()`: start TCP server, configure port (default 8484) via BepInEx
    config
  - `Update()`: process queued requests on the main thread, send responses
  - `OnDestroy()`: clean shutdown of TCP server
  - BepInEx config for port number, enable/disable toggle

- [ ] Implement `Server/TcpServer.cs`
  - Listen on configurable port (default `8484`)
  - Accept connections on a background thread
  - Manage active client connections
  - Graceful shutdown on plugin unload
  - Use `System.Net.Sockets.TcpListener` (available in .NET 3.5)

- [ ] Implement `Server/ClientHandler.cs`
  - Read JSON-line messages from the TCP stream
  - Parse into request objects
  - Enqueue requests for main-thread processing
  - Send responses back to the client
  - Handle client disconnection gracefully

- [ ] Implement `Protocol/` — message serialization
  - `Request.cs` and `Response.cs` — simple models for the JSON-line protocol
  - `MessageSerializer.cs` — JSON serialization compatible with .NET 3.5
    (no `System.Text.Json`; use a minimal built-in approach or a lightweight
    JSON library like `MiniJSON` that's commonly used in Unity)
  - Support message types: `query`, `response`, `error`

#### Plugin: State queries

- [ ] Implement `Queries/AgentQueries.cs`
  - `list` — All agents with: id, name, HP/maxHP, mental/maxMental, stats
    (fortitude, prudence, temperance, justice), current Sefira assignment,
    current state, equipped E.G.O. gifts, equipped E.G.O. weapon/suit
  - `get` — Single agent by ID with full detail
  - Source: `AgentManager.instance.agentList` (private — access via reflection
    or Harmony traverse)

- [ ] Implement `Queries/CreatureQueries.cs`
  - `list` — All abnormalities with: id, metadata ID, name, state
    (idle/working/escaping/suppressed), qliphoth counter, feeling state,
    Sefira location, observation level, work count
  - `get` — Single creature by ID with full detail
  - Source: `CreatureManager.instance` and its `creatureList`

- [ ] Implement `Queries/GameStateQueries.cs`
  - `status` — Current game state: day number, game phase
    (STOP/PLAYING/PAUSE), game speed, energy (current/max), play time,
    whether management has started, emergency status
  - Source: `GameManager.currentGameManager`, `PlayerModel.instance`

- [ ] Implement `Queries/SefiraQueries.cs`
  - `list` — All departments: name, enum value, open/closed, assigned agents,
    assigned abnormalities
  - Source: `SefiraManager.instance`

- [ ] Implement `Queries/QueryRouter.cs`
  - Route incoming query requests to the appropriate handler based on `target`
    field
  - Return structured error responses for unknown targets

#### Plugin: Tests

- [ ] Create `LobotomyPlaywright.Plugin.Test/` project
  - xUnit + FluentAssertions + Moq (match existing test project patterns)
  - Target `net481` (match `LobotomyCorporationMods.Test` pattern)
  - Add to `LobotomyCorporationMods.sln`

- [ ] Test message serialization round-trips
- [ ] Test query routing logic
- [ ] Test each query handler with mocked game managers
- [ ] Test TCP server connection handling (connect, send, receive, disconnect)
- [ ] Test main-thread queue processing
- [ ] Target 100% coverage on testable logic (exclude Unity runtime code with
  `[ExcludeFromCodeCoverage]` per project conventions)

#### Pi skill: Foundation

- [ ] Create `.pi/skills/lobotomy-playwright/SKILL.md`
  - Name: `lobotomy-playwright`
  - Description: Clearly states this is for observing and controlling a running
    Lobotomy Corporation game instance
  - Setup instructions for Python dependencies
  - Usage documentation for all available query commands
  - Reference to decompiled game source in `external/decompiled/Assembly-CSharp/`
    for understanding the game's data model when needed

- [ ] Implement `scripts/client.py` — TCP client library
  - Connect to game plugin on configurable host:port (default `localhost:8484`)
  - Send JSON-line requests, receive responses
  - Request ID generation and response correlation
  - Connection timeout and retry logic
  - Graceful error handling (game not running, connection refused, etc.)
  - Cross-platform (macOS, Windows, Linux)

- [ ] Implement `scripts/query.py` — state query CLI
  - `query.py agents` — List all agents
  - `query.py agents <id>` — Get agent details
  - `query.py creatures` — List all abnormalities
  - `query.py creatures <id>` — Get creature details
  - `query.py game` — Game state overview
  - `query.py departments` — Department status
  - Output: formatted, human-readable text (not raw JSON) so the agent can
    easily digest it
  - `--json` flag for raw JSON output when structured data is needed

- [ ] Python tests for the client library and CLI
  - Use pytest
  - Mock TCP connections for unit tests
  - Test serialization, error handling, timeout behavior
  - Test CLI output formatting

#### Integration verification

- [ ] End-to-end manual test: pi agent uses the skill to query game state
  from a running Lobotomy Corporation instance
- [ ] Document the setup process in the skill's `SKILL.md` and the plugin's
  `README.md`

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

- [ ] Implement `scripts/wait.py` in the pi skill
  - `wait.py event OnAgentDead --timeout 30` — Wait for a specific event
  - `wait.py condition "agents.count > 5"` — Wait for a state condition (poll-based)
  - Output the event data when the condition is met
  - Timeout with clear error message

- [ ] Add `lobcorp_wait` tool documentation to `SKILL.md`
- [ ] Tests for event subscription, streaming, and the wait CLI

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
- [ ] Implement `scripts/command.py` in the pi skill
  - `command.py assign-work --agent 3 --creature 100001 --work instinct`
  - `command.py pause`
  - `command.py set-agent-stats --agent 3 --hp 100 --mental 100`
  - `command.py fill-energy`
- [ ] Add `lobcorp_command` tool documentation to `SKILL.md`
- [ ] Tests for command routing, each command handler, and the command CLI

### Phase 4 — Mod testing workflows

- [ ] Design higher-level workflow tools
  - `scenario.py` — Run a scripted sequence of commands and assertions
  - Example: "Start day → assign Agent 3 to Instinct work on Scorched Girl →
    wait for work completion → verify gift probability increased (Bad Luck
    Protection mod) → verify no death warning appeared"

- [ ] Add workflow documentation to `SKILL.md`
  - Patterns for common mod testing scenarios
  - Examples using each existing mod in the repo

- [ ] Explore screenshot/visual state capture if feasible
  - Would allow the agent to "see" the game visually (useful for UI mods like
    GiftAlertIcon)

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
