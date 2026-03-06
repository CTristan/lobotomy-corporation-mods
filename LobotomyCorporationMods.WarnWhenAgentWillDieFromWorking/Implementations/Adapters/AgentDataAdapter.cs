// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.Adapters
{
    [AdapterClass]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class AgentDataAdapter : IAgentData
    {
        private readonly AgentModel _agent;

        public AgentDataAdapter([NotNull] AgentModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));
            _agent = agent;
        }

        public int fortitudeLevel => _agent.fortitudeLevel;
        public int prudenceLevel => _agent.prudenceLevel;
        public int temperanceLevel => _agent.temperanceLevel;
        public float fortitudeStat => _agent.fortitudeStat;
        public long instanceId => _agent.instanceId;

        public bool HasCrumblingArmor()
        {
            var crumblingArmorGiftsId = new List<int>
            {
                (int)EquipmentIds.CrumblingArmorGift1,
                (int)EquipmentIds.CrumblingArmorGift2,
                (int)EquipmentIds.CrumblingArmorGift3,
                (int)EquipmentIds.CrumblingArmorGift4,
            };

            return crumblingArmorGiftsId.Exists(_agent.HasEquipment);
        }

        public bool HasFairyFestivalEffect()
        {
            var effects = _agent.GetUnitBufList();

            return effects.OfType<FairyBuf>().Any();
        }

        public bool HasLaetitiaEffect()
        {
            var effects = _agent.GetUnitBufList();

            return effects.OfType<LittleWitchBuf>().Any();
        }

        public bool HasParasiteTreeEffect()
        {
            var effects = _agent.GetUnitBufList();

            return effects.OfType<YggdrasilBlessBuf>().Any();
        }

        public bool HasEquipment(int equipmentId)
        {
            return _agent.HasEquipment(equipmentId);
        }
    }
}
