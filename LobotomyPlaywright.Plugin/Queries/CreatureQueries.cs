// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LobotomyPlaywright.Queries
{
    /// <summary>
    /// Queries for creature (abnormality) information from CreatureManager.
    /// </summary>
    public static class CreatureQueries
    {
        private static FieldInfo s_creatureListField;
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
                    s_creatureListField = typeof(CreatureManager).GetField(
                        "creatureList",
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

        public static List<CreatureData> ListCreatures()
        {
            InitializeField();

            var creatures = new List<CreatureData>();
            var creatureManager = CreatureManager.instance;

            if (creatureManager == null)
            {
                throw new InvalidOperationException("CreatureManager.instance is null. Game may not be initialized.");
            }

            var creatureList = s_creatureListField?.GetValue(creatureManager) as List<CreatureModel>;

            if (creatureList != null)
            {
                foreach (var creature in creatureList)
                {
                    creatures.Add(CreateCreatureData(creature));
                }
            }

            return creatures;
        }

        public static CreatureData GetCreature(long instanceId)
        {
            var allCreatures = ListCreatures();
            return allCreatures.FirstOrDefault(c => c.InstanceId == instanceId);
        }

        private static CreatureData CreateCreatureData(CreatureModel creature)
        {
            if (creature == null)
            {
                return null;
            }

            return new CreatureData
            {
                InstanceId = creature.instanceId,
                MetadataId = creature.metadataId,
                Name = creature.metaInfo?.name ?? "Unknown",
                RiskLevel = creature.metaInfo?.riskLevel?.ToString() ?? "UNKNOWN",
                State = creature.state.ToString(),
                QliphothCounter = creature.qliphothCounter,
                MaxQliphothCounter = creature.script != null ? creature.script.GetQliphothCounterMax() : 0,
                FeelingState = creature.feelingState.ToString(),
                CurrentSefira = creature.sefiraNum ?? string.Empty,
                ObservationLevel = 0, // Access via reflection if needed
                WorkCount = creature.workCount,
                IsEscaping = creature.state == CreatureState.ESCAPE,
                IsSuppressed = creature.suppressReturnTimer > 0
            };
        }
    }
}
