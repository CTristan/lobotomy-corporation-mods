// SPDX-License-Identifier: MIT

using System.Reflection;

namespace LobotomyPlaywright.Events
{
    /// <summary>
    /// Creates structured event data objects from game event parameters.
    /// Uses reflection to extract relevant data from game objects.
    /// </summary>
    public static class EventDataFactory
    {
        /// <summary>
        /// Create event data for OnAgentDead event.
        /// </summary>
        public static object CreateAgentDeadEvent(object agentParam)
        {
            return new
            {
                agentId = GetIntProperty(agentParam, "instanceId", -1),
                agentName = GetStringProperty(agentParam, "name", "Unknown"),
                hp = GetFloatProperty(agentParam, "hp", 0),
                maxHp = GetFloatProperty(agentParam, "maxHp", 0),
                mental = GetFloatProperty(agentParam, "mental", 0),
                maxMental = GetFloatProperty(agentParam, "maxMental", 0),
                currentSefira = GetStringProperty(agentParam, "currentSefira", "Unknown")
            };
        }

        /// <summary>
        /// Create event data for OnAgentPanic event.
        /// </summary>
        public static object CreateAgentPanicEvent(object agentParam)
        {
            return new
            {
                agentId = GetIntProperty(agentParam, "instanceId", -1),
                agentName = GetStringProperty(agentParam, "name", "Unknown"),
                mental = GetFloatProperty(agentParam, "mental", 0),
                maxMental = GetFloatProperty(agentParam, "maxMental", 0),
                panicLevel = GetIntProperty(agentParam, "panicLevel", 1)
            };
        }

        /// <summary>
        /// Create event data for OnAgentPanicReturn event.
        /// </summary>
        public static object CreateAgentPanicReturnEvent(object agentParam)
        {
            return new
            {
                agentId = GetIntProperty(agentParam, "instanceId", -1),
                agentName = GetStringProperty(agentParam, "name", "Unknown"),
                mental = GetFloatProperty(agentParam, "mental", 0),
                maxMental = GetFloatProperty(agentParam, "maxMental", 0)
            };
        }

        /// <summary>
        /// Create event data for OnAgentPromote event.
        /// </summary>
        public static object CreateAgentPromoteEvent(object agentParam)
        {
            return new
            {
                agentId = GetIntProperty(agentParam, "instanceId", -1),
                agentName = GetStringProperty(agentParam, "name", "Unknown"),
                level = GetIntProperty(agentParam, "level", 1)
            };
        }

        /// <summary>
        /// Create event data for DeployAgent event.
        /// </summary>
        public static object CreateDeployAgentEvent(object unitParam)
        {
            return new
            {
                agentId = GetIntProperty(unitParam, "instanceId", -1),
                agentName = GetStringProperty(unitParam, "name", "Unknown"),
                currentSefira = GetStringProperty(unitParam, "currentSefira", "Unknown")
            };
        }

        /// <summary>
        /// Create event data for RemoveAgent event.
        /// </summary>
        public static object CreateRemoveAgentEvent(object unitParam)
        {
            return new
            {
                agentId = GetIntProperty(unitParam, "instanceId", -1),
                agentName = GetStringProperty(unitParam, "name", "Unknown"),
                wasDead = GetBoolProperty(unitParam, "isDead", false)
            };
        }

        /// <summary>
        /// Create event data for OnWorkStart event.
        /// </summary>
        public static object CreateWorkStartEvent(object creatureParam)
        {
            return new
            {
                creatureId = GetIntProperty(creatureParam, "instanceId", -1),
                creatureName = GetStringProperty(creatureParam, "name", "Unknown"),
                metadataId = GetIntProperty(creatureParam, "metaId", -1),
                riskLevel = GetStringProperty(creatureParam, "riskLevel", "Unknown")
            };
        }

        /// <summary>
        /// Create event data for OnWorkCoolTimeEnd event.
        /// </summary>
        public static object CreateWorkCoolTimeEndEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnReleaseWork event.
        /// </summary>
        public static object CreateReleaseWorkEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for WorkEndReport event.
        /// </summary>
        public static object CreateWorkEndReportEvent(object agentParam)
        {
            return new
            {
                agentId = GetIntProperty(agentParam, "instanceId", -1),
                agentName = GetStringProperty(agentParam, "name", "Unknown"),
                hp = GetFloatProperty(agentParam, "hp", 0),
                maxHp = GetFloatProperty(agentParam, "maxHp", 0),
                mental = GetFloatProperty(agentParam, "mental", 0),
                maxMental = GetFloatProperty(agentParam, "maxMental", 0),
                workCount = GetIntProperty(agentParam, "workCount", 0)
            };
        }

        /// <summary>
        /// Create event data for AddCreature event.
        /// </summary>
        public static object CreateAddCreatureEvent(object creatureParam)
        {
            return new
            {
                creatureId = GetIntProperty(creatureParam, "instanceId", -1),
                creatureName = GetStringProperty(creatureParam, "name", "Unknown"),
                metadataId = GetIntProperty(creatureParam, "metaId", -1),
                riskLevel = GetStringProperty(creatureParam, "riskLevel", "Unknown")
            };
        }

        /// <summary>
        /// Create event data for RemoveCreature event.
        /// </summary>
        public static object CreateRemoveCreatureEvent(object creatureParam)
        {
            return new
            {
                creatureId = GetIntProperty(creatureParam, "instanceId", -1),
                creatureName = GetStringProperty(creatureParam, "name", "Unknown"),
                wasSuppressed = GetBoolProperty(creatureParam, "isSuppressed", false)
            };
        }

        /// <summary>
        /// Create event data for OnCreatureSuppressed event.
        /// </summary>
        public static object CreateCreatureSuppressedEvent(object creatureParam)
        {
            return new
            {
                creatureId = GetIntProperty(creatureParam, "instanceId", -1),
                creatureName = GetStringProperty(creatureParam, "name", "Unknown"),
                metadataId = GetIntProperty(creatureParam, "metaId", -1)
            };
        }

        /// <summary>
        /// Create event data for EscapeCreature event.
        /// </summary>
        public static object CreateEscapeCreatureEvent(object creatureParam)
        {
            return new
            {
                creatureId = GetIntProperty(creatureParam, "instanceId", -1),
                creatureName = GetStringProperty(creatureParam, "name", "Unknown"),
                metadataId = GetIntProperty(creatureParam, "metaId", -1),
                riskLevel = GetStringProperty(creatureParam, "riskLevel", "Unknown")
            };
        }

        /// <summary>
        /// Create event data for CreatureObserveLevelAdded event.
        /// </summary>
        public static object CreateCreatureObserveLevelEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnOrdealStarted event.
        /// </summary>
        public static object CreateOrdealStartedEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnOrdealActivated event.
        /// </summary>
        public static object CreateOrdealActivatedEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnEmergencyLevelChanged event.
        /// </summary>
        public static object CreateEmergencyLevelChangedEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OrdealEnd event.
        /// </summary>
        public static object CreateOrdealEndEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnStageStart event.
        /// </summary>
        public static object CreateStageStartEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnStageEnd event.
        /// </summary>
        public static object CreateStageEndEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnNextDay event.
        /// </summary>
        public static object CreateNextDayEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnFailStage event.
        /// </summary>
        public static object CreateFailStageEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for OnGetEGOgift event.
        /// </summary>
        public static object CreateGetEGOgiftEvent(object giftParam)
        {
            return new
            {
                giftId = GetIntProperty(giftParam, "instanceId", -1),
                giftName = GetStringProperty(giftParam, "name", "Unknown"),
                metadataId = GetIntProperty(giftParam, "metaId", -1)
            };
        }

        /// <summary>
        /// Create event data for OnChangeGift event.
        /// </summary>
        public static object CreateChangeGiftEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for MakeEquipment event.
        /// </summary>
        public static object CreateMakeEquipmentEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for UpdateEnergy event.
        /// </summary>
        public static object CreateUpdateEnergyEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for SefiraEnabled event.
        /// </summary>
        public static object CreateSefiraEnabledEvent()
        {
            return new { };
        }

        /// <summary>
        /// Create event data for SefiraDisabled event.
        /// </summary>
        public static object CreateSefiraDisabledEvent()
        {
            return new { };
        }

        // Helper methods for reflection

        private static string GetStringProperty(object obj, string propertyName, string defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            try
            {
                var property = obj.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(obj, null);
                    return value?.ToString() ?? defaultValue;
                }
            }
            catch
            {
                // Ignore reflection errors
            }

            return defaultValue;
        }

        private static int GetIntProperty(object obj, string propertyName, int defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            try
            {
                var property = obj.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(obj, null);
                    if (value is int intValue)
                    {
                        return intValue;
                    }
                }
            }
            catch
            {
                // Ignore reflection errors
            }

            return defaultValue;
        }

        private static float GetFloatProperty(object obj, string propertyName, float defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            try
            {
                var property = obj.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(obj, null);
                    if (value is float floatValue)
                    {
                        return floatValue;
                    }
                }
            }
            catch
            {
                // Ignore reflection errors
            }

            return defaultValue;
        }

        private static bool GetBoolProperty(object obj, string propertyName, bool defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            try
            {
                var property = obj.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(obj, null);
                    if (value is bool boolValue)
                    {
                        return boolValue;
                    }
                }
            }
            catch
            {
                // Ignore reflection errors
            }

            return defaultValue;
        }
    }
}
