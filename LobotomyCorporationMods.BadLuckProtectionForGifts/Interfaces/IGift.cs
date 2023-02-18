// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    internal interface IGift
    {
        List<IAgent> GetAgents();
        string GetName();

        IAgent GetOrAddAgent(long agentId);
    }
}
