// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using LobotomyPlaywright.JsonModels;
using LobotomyPlaywright.Protocol;
using UnityEngine;

// Use alias to avoid conflicts with Unity's Debug
using TcpServer = LobotomyPlaywright.Server.TcpServer;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Player action simulation commands - what a player would click.
    ///
    /// Expected game field names (via reflection):
    /// - AgentModel: instId, targetCreature, workType, state, currentSefira
    /// - CreatureModel: instId, qliphothCounter, isSuppressed, state
    /// - GameManager: gameState, currentGameManager
    /// - NoticeName: WorkStart (and other notice name fields)
    ///
    /// Note: These field names are based on Lobotomy Corporation game internals and may change with game updates.
    /// </summary>
    public static class PlayerActionCommands
    {
        /// <summary>
        /// Pause the game.
        /// </summary>
        public static Response HandlePause(Request request)
        {
            try
            {
                var gameManager = GetGameManager();
                if (gameManager == null)
                {
                    return Response.CreateError(request.Id, "GameManager not available", "NOT_AVAILABLE");
                }

                var gameState = GetPrivateField<int>(gameManager, "gameState");
                if (gameState == 2) // Already paused
                {
                    return Response.CreateSuccess(request.Id, new { result = "already_paused" });
                }

                SetPrivateField(gameManager, "gameState", 2); // PAUSE state
                TcpServer.LogDebug("[LobotomyPlaywright] Game paused");
                return Response.CreateSuccess(request.Id, new { result = "paused" });
            }
            catch (Exception ex)
            {
                return Response.CreateError(request.Id, $"Failed to pause: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Unpause the game.
        /// </summary>
        public static Response HandleUnpause(Request request)
        {
            try
            {
                var gameManager = GetGameManager();
                if (gameManager == null)
                {
                    return Response.CreateError(request.Id, "GameManager not available", "NOT_AVAILABLE");
                }

                var gameState = GetPrivateField<int>(gameManager, "gameState");
                if (gameState != 2) // Not paused
                {
                    return Response.CreateSuccess(request.Id, new { result = "already_playing" });
                }

                SetPrivateField(gameManager, "gameState", 1); // PLAYING state
                TcpServer.LogDebug("[LobotomyPlaywright] Game unpaused");
                return Response.CreateSuccess(request.Id, new { result = "unpaused" });
            }
            catch (Exception ex)
            {
                return Response.CreateError(request.Id, $"Failed to unpause: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Assign an agent to work on a creature.
        /// </summary>
        public static Response HandleAssignWork(Request request)
        {
            var paramsObj = GetParams<AssignWorkParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for assign-work", "INVALID_PARAMS");
            }

            if (paramsObj.agentId <= 0 || paramsObj.creatureId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid agentId or creatureId", "INVALID_PARAMS");
            }

            if (string.IsNullOrEmpty(paramsObj.workType))
            {
                return Response.CreateError(request.Id, "Missing workType", "INVALID_PARAMS");
            }

            try
            {
                var agent = GetAgentById(paramsObj.agentId);
                if (agent == null)
                {
                    return Response.CreateError(request.Id, $"Agent not found: {paramsObj.agentId}", "NOT_FOUND");
                }

                var creature = GetCreatureById(paramsObj.creatureId);
                if (creature == null)
                {
                    return Response.CreateError(request.Id, $"Creature not found: {paramsObj.creatureId}", "NOT_FOUND");
                }

                // Get the work type enum value
                var workTypeEnum = ParseWorkType(paramsObj.workType);
                if (workTypeEnum == null)
                {
                    return Response.CreateError(request.Id, $"Invalid work type: {paramsObj.workType}", "INVALID_PARAMS");
                }

                // Set agent's current target creature
                SetPrivateField(agent, "targetCreature", creature);
                SetPrivateField(agent, "workType", (int)workTypeEnum);

                // Set agent state to working
                SetPrivateField(agent, "state", 1); // WORKING state

                // Send work start notice
                var noticeName = GetNoticeName("WorkStart");
                if (noticeName != null)
                {
                    var noticeType = typeof(Notice);
                    var sendMethod = noticeType.GetMethod("Send", BindingFlags.Static | BindingFlags.Public);
                    sendMethod?.Invoke(null, new[] { noticeName, agent });
                }

                TcpServer.LogDebug($"[LobotomyPlaywright] Assigned agent {paramsObj.agentId} to creature {paramsObj.creatureId} for {paramsObj.workType}");
                return Response.CreateSuccess(request.Id, new { result = "assigned", agentId = paramsObj.agentId, creatureId = paramsObj.creatureId, workType = paramsObj.workType });
            }
            catch (Exception ex)
            {
                return Response.CreateError(request.Id, $"Failed to assign work: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Deploy an agent to a department.
        /// </summary>
        public static Response HandleDeployAgent(Request request)
        {
            var paramsObj = GetParams<DeployAgentParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for deploy-agent", "INVALID_PARAMS");
            }

            if (paramsObj.agentId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid agentId", "INVALID_AGENT_ID");
            }

            if (string.IsNullOrEmpty(paramsObj.sefira))
            {
                return Response.CreateError(request.Id, "Missing sefira", "INVALID_PARAMS");
            }

            try
            {
                var agent = GetAgentById(paramsObj.agentId);
                if (agent == null)
                {
                    return Response.CreateError(request.Id, $"Agent not found: {paramsObj.agentId}", "NOT_FOUND");
                }

                var sefiraEnum = ParseSefira(paramsObj.sefira);
                if (sefiraEnum == null)
                {
                    return Response.CreateError(request.Id, $"Invalid sefira: {paramsObj.sefira}", "INVALID_PARAMS");
                }

                // Set agent's current sefira
                SetPrivateField(agent, "currentSefira", (int)sefiraEnum);

                // Set agent state to idle (deployed but not working)
                SetPrivateField(agent, "state", 0); // IDLE state

                // Clear target creature
                SetPrivateField(agent, "targetCreature", null);

                TcpServer.LogDebug($"[LobotomyPlaywright] Deployed agent {paramsObj.agentId} to {paramsObj.sefira}");
                return Response.CreateSuccess(request.Id, new { result = "deployed", agentId = paramsObj.agentId, sefira = paramsObj.sefira });
            }
            catch (Exception ex)
            {
                return Response.CreateError(request.Id, $"Failed to deploy agent: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Recall an agent (return to standby).
        /// </summary>
        public static Response HandleRecallAgent(Request request)
        {
            var paramsObj = GetParams<RecallAgentParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for recall-agent", "INVALID_PARAMS");
            }

            if (paramsObj.agentId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid agentId", "INVALID_AGENT_ID");
            }

            try
            {
                var agent = GetAgentById(paramsObj.agentId);
                if (agent == null)
                {
                    return Response.CreateError(request.Id, $"Agent not found: {paramsObj.agentId}", "NOT_FOUND");
                }

                // Clear target creature
                SetPrivateField(agent, "targetCreature", null);

                // Clear current sefira (return to base)
                SetPrivateField(agent, "currentSefira", -1);

                // Set agent state to idle
                SetPrivateField(agent, "state", 0); // IDLE state

                TcpServer.LogDebug($"[LobotomyPlaywright] Recalled agent {paramsObj.agentId}");
                return Response.CreateSuccess(request.Id, new { result = "recalled", agentId = paramsObj.agentId });
            }
            catch (Exception ex)
            {
                return Response.CreateError(request.Id, $"Failed to recall agent: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Command suppression on a creature.
        /// </summary>
        public static Response HandleSuppress(Request request)
        {
            var paramsObj = GetParams<SuppressParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for suppress", "INVALID_PARAMS");
            }

            if (paramsObj.creatureId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid creatureId", "INVALID_CREATURE_ID");
            }

            try
            {
                var creature = GetCreatureById(paramsObj.creatureId);
                if (creature == null)
                {
                    return Response.CreateError(request.Id, $"Creature not found: {paramsObj.creatureId}", "NOT_FOUND");
                }

                // Set creature to suppressed state
                SetPrivateField(creature, "isSuppressed", true);
                SetPrivateField(creature, "state", 3); // SUPPRESSED state

                // Reset qliphoth counter
                SetPrivateField(creature, "qliphothCounter", 0);

                TcpServer.LogDebug($"[LobotomyPlaywright] Suppressed creature {paramsObj.creatureId}");
                return Response.CreateSuccess(request.Id, new { result = "suppressed", creatureId = paramsObj.creatureId });
            }
            catch (Exception ex)
            {
                return Response.CreateError(request.Id, $"Failed to suppress: {ex.Message}", "COMMAND_ERROR");
            }
        }

        #region Helper Methods

        private static T GetParams<T>(Request request) where T : class
        {
            if (request.Params == null)
            {
                return null;
            }

            try
            {
                var paramsObj = Activator.CreateInstance<T>();
                var type = typeof(T);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (request.Params.ContainsKey(field.Name))
                    {
                        var value = request.Params[field.Name];
                        if (value != null)
                        {
                            object convertedValue;
                            if (field.FieldType == typeof(long))
                            {
                                convertedValue = Convert.ToInt64(value);
                            }
                            else if (field.FieldType == typeof(int))
                            {
                                convertedValue = Convert.ToInt32(value);
                            }
                            else if (field.FieldType == typeof(float))
                            {
                                convertedValue = Convert.ToSingle(value);
                            }
                            else if (field.FieldType == typeof(bool))
                            {
                                convertedValue = Convert.ToBoolean(value);
                            }
                            else if (field.FieldType == typeof(string))
                            {
                                convertedValue = value.ToString();
                            }
                            else
                            {
                                convertedValue = value;
                            }
                            field.SetValue(paramsObj, convertedValue);
                        }
                    }
                }

                return paramsObj;
            }
            catch
            {
                return null;
            }
        }

        private static object GetAgentById(long agentId)
        {
            try
            {
                var agentManager = GetAgentManager();
                if (agentManager == null) return null;

                var agentList = GetPrivateField<List<object>>(agentManager, "agentList");
                if (agentList == null) return null;

                foreach (var agent in agentList)
                {
                    var instId = GetPrivateField<long>(agent, "instId");
                    if (instId == agentId)
                    {
                        return agent;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static object GetCreatureById(long creatureId)
        {
            try
            {
                var creatureManager = GetCreatureManager();
                if (creatureManager == null) return null;

                var creatureList = GetPrivateField<List<object>>(creatureManager, "creatureList");
                if (creatureList == null) return null;

                foreach (var creature in creatureList)
                {
                    var instId = GetPrivateField<long>(creature, "instId");
                    if (instId == creatureId)
                    {
                        return creature;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static object GetAgentManager()
        {
            var type = typeof(AgentManager);
            var field = type.GetField("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(null);
        }

        private static object GetCreatureManager()
        {
            var type = typeof(CreatureManager);
            var field = type.GetField("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(null);
        }

        private static object GetGameManager()
        {
            var type = typeof(GameManager);
            var field = type.GetField("currentGameManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(null);
        }

        private static object GetNoticeName(string name)
        {
            var type = typeof(NoticeName);
            var field = type.GetField(name, BindingFlags.Static | BindingFlags.Public);
            return field?.GetValue(null);
        }

        private static int? ParseWorkType(string workType)
        {
            // Map common work type names to their enum values
            var workTypes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "instinct", 0 },
                { "insight", 1 },
                { "attachment", 2 },
                { "repression", 3 },
                { "escape", 4 },
                { " Observation", 0 }, // Alternate names
                { " Entry", 1 },
                { " Breakthrough", 2 },
                { " Realization", 3 }
            };

            if (workTypes.TryGetValue(workType, out int value))
            {
                return value;
            }

            return null;
        }

        private static int? ParseSefira(string sefira)
        {
            // Sefira enum values
            var sefiras = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "BINAH", 0 },
                { "CHESED", 1 },
                { "GEBURAH", 2 },
                { "TIPHERETH", 3 },
                { "NETZACH", 4 },
                { "YESOD", 5 },
                { "MALKUTH", 6 }
            };

            if (sefiras.TryGetValue(sefira, out int value))
            {
                return value;
            }

            return null;
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                TcpServer.LogDebug($"[LobotomyPlaywright] Field '{fieldName}' not found on type '{type.Name}'");
                return default;
            }
            var value = field.GetValue(obj);
            if (value is T typedValue) return typedValue;
            return default;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                TcpServer.LogDebug($"[LobotomyPlaywright] Field '{fieldName}' not found on type '{type.Name}' - set failed");
            }
        }

        #endregion
    }
}
