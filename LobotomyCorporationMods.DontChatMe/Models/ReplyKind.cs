// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>The three reply types the mod sends back to the chat-side server.</summary>
    public enum ReplyKind
    {
        DispatchAck = 0,
        EffectExecuted = 1,
        EffectFailed = 2,
    }
}
