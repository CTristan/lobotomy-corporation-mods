// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IGift
    {
        ICollection<IAgent> GetAgents();
        string GetName();

        IAgent GetOrAddAgent(long agentId);
    }
}
