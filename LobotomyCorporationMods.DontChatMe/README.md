# Don't Chat Me

A mod for Lobotomy Corporation that lets viewers in your Twitch chat redeem effects that play out in your facility while you manage it.

This mod is the in-game receiver. It connects to a separate chat-side service (such as [HemoGrace](https://github.com/CTristan/hemograce)) that decides which effects to send and when. Without that service running and reachable, this mod does nothing.

## What it does

When a viewer redeems an effect on the chat side, the service sends the redemption to the mod. The mod runs the effect on the next game tick and replies with the outcome. The chat side uses the outcome to refund or confirm the redemption.

Nine effects ship in v1:

- **Random Meltdown** — one random abnormality goes into meltdown
- **Kill Random Agent** — a random living agent dies; their equipment stays
- **Random Agent Panic** — a random controllable agent panics, sanity drained to zero
- **Add Energy** — facility energy goes up by the configured amount
- **Remove Energy** — facility energy goes down by the configured amount
- **Add Money** — LOB Points go up by the configured amount
- **Show System Message** — the viewer's chat name appears in the system log
- **Set Game Speed** — game speed jumps to 2x
- **Escape Random Creature** — a random contained creature is released; gated behind an opt-in danger flag

Each effect has a built-in cooldown so spam in chat does not overwhelm the game.

## On-screen status

A small overlay at the top center of the screen shows the mod's current state:

- A colored dot and label for the connection: gray "disabled", red "disconnected", amber "connecting…", or green "connected"
- The number of effects waiting in line, when the queue is not empty
- The slug of the most recent effect that ran

Press **F8** at any time to hide or show the overlay. Hiding it does not disconnect the mod — it only stops drawing the indicator.

## Install

1. Install [Lobotomy Mod Manager](https://www.nexusmods.com/site/mods/765) or [Basemod](https://www.nexusmods.com/lobotomycorporation/mods/2).
2. Drop `LobotomyCorporationMods.DontChatMe.dll` and its `Info/` and `Localize/` folders into your `BaseMods/DontChatMe/` directory.
3. Install [ConfigurationManager](https://www.nexusmods.com/lobotomycorporation/mods/72) (recommended; lets you change the URL and token in-game).
4. Start the chat-side service and copy its URL and auth token.

## Configure

In ConfigurationManager (press F1 by default):

- **Server URL** — `wss://<your chat-side host>/mod/socket` (or `ws://` for local testing)
- **Auth Token** — the secret your chat-side service expects
- **Enabled** — master switch; off keeps the mod disconnected
- **Allow Danger Effects** — when off, the creature-escape effect is rejected
- **Energy Amount / Money Amount** — how much each economy effect grants
- **Max In-Flight Redemptions** — the queue cap (32 by default)
- **Global Cooldown (seconds)** — minimum gap between any two effects; 0 disables

## How it talks to the chat side

The wire is a plain WebSocket carrying one JSON object per text frame. The mod is the client; the chat-side service is the server. See the project plan for the full contract.

## Caveats

This mod ships before the chat-side service it depends on. Once that side is up, redemptions will flow. Until then, the mod loads, registers its config, and waits.

Effects are intentionally noisy — this is a chaos mod for streams, not a balance tweak. Don't run it during a serious save.

## End-to-end tests

In-game e2e tests for Don't Chat Me live in the [open-lobotomy-e2e](https://github.com/open-lobotomy/open-lobotomy-e2e) repo. That repo deploys the mod via The Silent Orchestrator, hosts a stub chat-side WebSocket server in-process, and verifies the round-trip end to end.

## License

MIT.

`SPDX-License-Identifier: MIT`
