// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace LobotomyCorporationMods.Test.Parameters
{
    internal sealed class AgentModelCreationParameters
    {
        public AgentName AgentName { get; set; }
        public List<UnitBuf> BufList { get; set; }
        public UnitEquipSpace Equipment { get; set; }
        public long InstanceId { get; set; } = 1L;
        public string Name { get; set; } = "";
        public WorkerPrimaryStat PrimaryStat { get; set; }
        public WorkerSprite.WorkerSprite SpriteData { get; set; }
        public List<UnitStatBuf> StatBufList { get; set; }
    }
}
