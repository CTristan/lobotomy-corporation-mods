# Changelog

All notable changes to this mod will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this mod adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - Unreleased

### Added

- Initial release. Connects to a chat-side service over WebSocket and runs in-game effects redeemed by chat viewers.
- Nine starter effects: Random Meltdown, Kill Random Agent, Random Agent Panic, Add Energy, Remove Energy, Add Money, Show System Message, Set Game Speed, and Escape Random Creature.
- Built-in Settings window for the connection fields (Server URL, Auth Token, Enabled). Open it from the gear button on the status overlay or by pressing F9. Server URL and Auth Token are hidden by default for safe configuration on stream.
- Configurable effect amounts, queue capacity, and global cooldown via ConfigurationManager (optional).
- "Allow Danger Effects" opt-in so creature-escape requests can be rejected by default.
- Per-effect cooldowns to keep chat spam from overwhelming the game.
- On-screen status overlay at the top center of the screen showing the connection state, the number of effects waiting in line, and the most recently run effect. Press F8 to hide or show the overlay during gameplay.
