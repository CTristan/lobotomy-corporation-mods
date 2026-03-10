// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LobotomyPlaywright.Queries
{
    /// <summary>
    /// Queries for agent information from AgentManager.
    /// </summary>
    public static class AgentQueries
    {
        private static FieldInfo s_agentListField;
        private static FieldInfo s_agentListSpareField;
        private static bool s_fieldsInitialized;
        private static readonly object s_initLock = new object();

        private static void InitializeFields()
        {
            if (s_fieldsInitialized)
            {
                return;
            }

            lock (s_initLock)
            {
                if (s_fieldsInitialized)
                {
                    return;
                }

                try
                {
                    s_agentListField = typeof(AgentManager).GetField(
                        "agentList",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );

                    s_agentListSpareField = typeof(AgentManager).GetField(
                        "agentListSpare",
                        BindingFlags.Public | BindingFlags.Instance
                    );
                }
                catch
                {
                    // Fields will remain null if initialization fails
                }

                s_fieldsInitialized = true;
            }
        }

        public static List<AgentData> ListAgents()
        {
            InitializeFields();

            var agents = new List<AgentData>();
            var agentManager = AgentManager.instance ?? throw new InvalidOperationException("AgentManager.instance is null. Game may not be initialized.");
            if (s_agentListField?.GetValue(agentManager) is List<AgentModel> agentList)
            {
                foreach (var agent in agentList)
                {
                    agents.Add(CreateAgentData(agent));
                }
            }

            if (s_agentListSpareField?.GetValue(agentManager) is List<AgentModel> spareList)
            {
                foreach (var agent in spareList)
                {
                    agents.Add(CreateAgentData(agent));
                }
            }

            return agents;
        }

        public static AgentData GetAgent(long instanceId)
        {
            var allAgents = ListAgents();
            return allAgents.FirstOrDefault(a => a.InstanceId == instanceId);
        }

        private static AgentData CreateAgentData(AgentModel agent)
        {
            if (agent == null)
            {
                return null;
            }

            var gifts = new List<string>();
            var allGifts = agent.GetAllGifts();
            if (allGifts != null)
            {
                foreach (var gift in allGifts)
                {
                    if (gift != null && gift.metaInfo != null)
                    {
                        gifts.Add(gift.metaInfo.id.ToString());
                    }
                }
            }

            return new AgentData
            {
                InstanceId = agent.instanceId,
                Name = agent.name ?? "Unknown",
                Hp = agent.hp,
                MaxHp = agent.maxHp,
                Mental = agent.mental,
                MaxMental = agent.maxMental,
                Fortitude = agent.fortitudeStat,
                Prudence = agent.prudenceStat,
                Temperance = agent.temperanceStat,
                Justice = agent.justiceStat,
                CurrentSefira = agent.currentSefiraEnum.ToString(),
                State = GetAgentState(agent),
                GiftIds = gifts.ToArray(),
                WeaponId = agent.Equipment?.weapon?.metaInfo != null ? agent.Equipment.weapon.metaInfo.id.ToString() : string.Empty,
                ArmorId = agent.Equipment?.armor?.metaInfo != null ? agent.Equipment.armor.metaInfo.id.ToString() : string.Empty,
                IsDead = agent.IsDead(),
                IsPanicking = agent.panicValue > 0
            };
        }

        private static string GetAgentState(AgentModel agent)
        {
            if (agent.IsDead())
            {
                return "DEAD";
            }
            if (agent.panicValue > 0)
            {
                return "PANIC";
            }
            if (agent.stunTime > 0)
            {
                return "STUNNED";
            }
            if (agent.currentSkill != null)
            {
                return "WORKING";
            }
            return "IDLE";
        }
    }
}
