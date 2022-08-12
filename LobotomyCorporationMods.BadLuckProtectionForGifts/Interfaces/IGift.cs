// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    internal interface IGift
    {
        string GetName();
        List<IAgent> GetAgents();

        IAgent GetOrAddAgent(long agentId);
    }
}
