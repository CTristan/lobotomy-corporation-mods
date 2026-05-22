// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>String constants for the <c>type</c> field of every frame on the wire.</summary>
    public static class WireTypes
    {
        // Outbound (mod → server)
        public const string Hello = "hello";
        public const string Pong = "pong";
        public const string DispatchAck = "dispatch_ack";
        public const string EffectExecuted = "effect_executed";
        public const string EffectFailed = "effect_failed";
        public const string EffectState = "effect_state";

        // Inbound (server → mod)
        public const string Welcome = "welcome";
        public const string Error = "error";
        public const string Ping = "ping";
        public const string EffectDispatched = "effect_dispatched";
    }
}
