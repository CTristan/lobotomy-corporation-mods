// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#endregion

namespace LobotomyCorporationMods.Playwright.Queries
{
    /// <summary>
    /// Queries for department (Sefira) information from SefiraManager.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SefiraQueries
    {
        private static FieldInfo s_officerCntField;
        private static bool s_fieldInitialized;
        private static readonly object s_initLock = new object();

        private static void InitializeField()
        {
            if (s_fieldInitialized)
            {
                return;
            }

            lock (s_initLock)
            {
                if (s_fieldInitialized)
                {
                    return;
                }

                try
                {
                    s_officerCntField = typeof(Sefira).GetField(
                        "officerCnt",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                }
                catch
                {
                    // Field will remain null if initialization fails
                }

                s_fieldInitialized = true;
            }
        }

        public static ICollection<SefiraData> ListSefira()
        {
            var sefiraList = new List<SefiraData>();
            var sefiraManager = SefiraManager.instance ?? throw new InvalidOperationException("SefiraManager.instance is null. Game may not be initialized.");

            // Iterate through all SefiraEnum values
            foreach (SefiraEnum sefiraEnum in Enum.GetValues(typeof(SefiraEnum)))
            {
                if (sefiraEnum == SefiraEnum.DUMMY)
                {
                    continue;
                }

                var sefira = sefiraManager.GetSefira(sefiraEnum);
                if (sefira != null)
                {
                    sefiraList.Add(CreateSefiraData(sefira));
                }
            }

            return sefiraList;
        }

        public static SefiraData GetSefira(SefiraEnum sefiraEnum)
        {
            var sefiraManager = SefiraManager.instance ?? throw new InvalidOperationException("SefiraManager.instance is null. Game may not be initialized.");
            var sefira = sefiraManager.GetSefira(sefiraEnum) ?? throw new ArgumentException($"Sefira {sefiraEnum} not found.");
            return CreateSefiraData(sefira);
        }

        private static SefiraData CreateSefiraData(Sefira sefira)
        {
            InitializeField();

            if (sefira == null)
            {
                return null;
            }

            var agentIds = new List<long>();
            if (sefira.agentList != null)
            {
                foreach (var agent in sefira.agentList)
                {
                    if (agent != null)
                    {
                        agentIds.Add(agent.instanceId);
                    }
                }
            }

            var creatureIds = new List<long>();
            if (sefira.creatureList != null)
            {
                foreach (var creature in sefira.creatureList)
                {
                    if (creature != null)
                    {
                        creatureIds.Add(creature.instanceId);
                    }
                }
            }

            var officerCount = 0;
            if (s_officerCntField != null)
            {
                officerCount = (int)s_officerCntField.GetValue(sefira);
            }

            return new SefiraData
            {
                Name = sefira.name ?? string.Empty,
                SefiraEnum = sefira.sefiraEnum.ToString(),
                IsOpen = sefira.openLevel > 0,
                OpenLevel = sefira.openLevel,
                AgentIds = agentIds,
                CreatureIds = creatureIds,
                OfficerCount = officerCount
            };
        }
    }
}
