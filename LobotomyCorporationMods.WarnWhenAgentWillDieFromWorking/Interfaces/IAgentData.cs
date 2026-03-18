// SPDX-License-Identifier: MIT

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces
{
    public interface IAgentData
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
