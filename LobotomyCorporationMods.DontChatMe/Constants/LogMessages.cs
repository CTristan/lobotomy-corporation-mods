// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    internal static class LogMessages
    {
        internal const string TransportConnected = "DontChatMe transport connected.";
        internal const string TransportDisconnected = "DontChatMe transport disconnected.";
        internal const string TransportReconnecting =
            "DontChatMe transport reconnecting in {0} ms (attempt {1}).";
        internal const string ParseFailure = "DontChatMe failed to parse inbound frame: {0}";
        internal const string EffectExecutionFailed = "DontChatMe effect '{0}' failed: {1}";
        internal const string QueueOverflow =
            "DontChatMe queue at capacity ({0}); rejecting redemption {1}.";
        internal const string UnknownInboundType = "DontChatMe received unknown frame type '{0}'.";
    }
}
