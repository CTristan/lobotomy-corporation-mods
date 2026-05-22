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
- A gear button to the right of the label opens the Settings window

Press **F8** at any time to hide or show the overlay. Hiding it does not disconnect the mod — it only stops drawing the indicator. The Settings window stays reachable via the F9 keybind even when the overlay is hidden.

## Install

1. Install [Lobotomy Mod Manager](https://www.nexusmods.com/site/mods/765) or [Basemod](https://www.nexusmods.com/lobotomycorporation/mods/2).
2. Drop `LobotomyCorporationMods.DontChatMe.dll` and its `Info/` and `Localize/` folders into your `BaseMods/DontChatMe/` directory.
3. Start the chat-side service and copy its URL and auth token.

[ConfigurationManager](https://www.nexusmods.com/lobotomycorporation/mods/72) is optional. The mod ships with a built-in Settings window for the connection fields. Install ConfigurationManager only if you want to tweak the effect amounts, queue cap, or cooldown.

## Configure

Click the gear button on the overlay (or press **F9**) to open the Settings window. The window edits the three connection fields:

- **Server URL** — `wss://<your chat-side host>/mod/socket` (or `ws://` for local testing)
- **Auth Token** — the secret your chat-side service expects
- **Enabled** — master switch; off keeps the mod disconnected

Click **Apply** to write the values and reconnect; click **Cancel** to discard your edits.

### Streaming safety

The Server URL and Auth Token are hidden by default every time the window opens. Click **Show** next to either field to reveal it for review; the field re-masks the next time you open Settings, so a forgotten reveal does not leak across sessions. The window also closes on a successful Apply.

### Power-user settings (ConfigurationManager)

Install ConfigurationManager and press F1 to reach the remaining settings:

- **Allow Danger Effects** — when off, the creature-escape effect is rejected
- **Energy Amount / Money Amount** — how much each economy effect grants
- **Max In-Flight Redemptions** — the queue cap (32 by default)
- **Global Cooldown (seconds)** — minimum gap between any two effects; 0 disables

Both Settings windows write to the same on-disk config file, so changes from one show up in the other.

## How it talks to the chat side

The wire is a plain WebSocket carrying one JSON object per text frame. The mod is the client; the chat-side service is the server. See the project plan for the full contract.

## Caveats

This mod ships before the chat-side service it depends on. Once that side is up, redemptions will flow. Until then, the mod loads, registers its config, and waits.

Effects are intentionally noisy — this is a chaos mod for streams, not a balance tweak. Don't run it during a serious save.

## End-to-end tests

In-game e2e tests for Don't Chat Me live in the [lobotomy-corporation-mods-e2e](https://github.com/CTristan/lobotomy-corporation-mods-e2e) repo. That repo deploys the mod via The Silent Orchestrator, hosts a stub chat-side WebSocket server in-process, and verifies the round-trip end to end.

## License

MIT.

`SPDX-License-Identifier: MIT`
