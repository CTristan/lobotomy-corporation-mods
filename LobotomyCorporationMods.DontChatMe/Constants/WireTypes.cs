// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>String constants for the <c>type</c> field of every frame on the wire.</summary>
    /// <remarks>
    /// Canonical protocol: see <c>apps/hemograce_web/docs/game_mod_protocol.md</c> in
    /// the hemograce repository. Both sides of the WebSocket conform to that spec; this
    /// class holds the type strings the mod emits or matches.
    /// </remarks>
    public static class WireTypes
    {
        // Outbound (mod → server)
        public const string Hello = "hello";
        public const string EffectResponse = "effect_response";
        public const string KeepAlive = "keep_alive";
        public const string EffectState = "effect_state";
        public const string GameState = "game_state";

        // Inbound (server → mod)
        public const string Welcome = "welcome";
        public const string EffectDispatch = "effect_dispatch";
        public const string Error = "error";
    }
}
