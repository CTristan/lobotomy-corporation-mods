// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces
{
    internal interface IAgentData
    {
        int fortitudeLevel { get; }
        int prudenceLevel { get; }
        int temperanceLevel { get; }
        float fortitudeStat { get; }
        long instanceId { get; }

        bool HasCrumblingArmor();
        bool HasFairyFestivalEffect();
        bool HasLaetitiaEffect();
        bool HasParasiteTreeEffect();
        bool HasEquipment(int equipmentId);
    }
}
