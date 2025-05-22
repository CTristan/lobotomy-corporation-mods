// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.DontChatMe.Models.EffectMessages;

namespace LobotomyCorporationMods.DontChatMe.Interfaces
{
    public interface IEffectExecutor
    {
        EffectResponse Execute(EffectRequest message);
    }
}
