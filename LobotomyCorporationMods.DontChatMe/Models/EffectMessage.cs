// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.DontChatMe.Constants;

namespace LobotomyCorporationMods.DontChatMe.Models
{
    public abstract class EffectMessage : WebSocketMessage
    {
        public sealed override string MessageType { get => MessageTypes.Effect; }
    }
}
