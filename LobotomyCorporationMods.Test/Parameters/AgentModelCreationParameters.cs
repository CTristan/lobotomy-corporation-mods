// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace LobotomyCorporationMods.Test.Parameters
{
    internal sealed class AgentModelCreationParameters
    {
        internal AgentName? AgentName { get; set; }
        internal List<UnitBuf>? BufList { get; set; }
        internal UnitEquipSpace? Equipment { get; set; }
        internal long InstanceId { get; set; } = 1L;
        internal string Name { get; set; } = "";
        internal WorkerPrimaryStat? PrimaryStat { get; set; }
        internal WorkerSprite.WorkerSprite? SpriteData { get; set; }
        internal List<UnitStatBuf>? StatBufList { get; set; }
    }
}
