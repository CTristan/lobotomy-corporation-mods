// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LobotomyCorporationMods.Playwright.JsonModels;

// Use alias to avoid conflicts with Unity's Debug
using TcpServer = LobotomyCorporationMods.Playwright.Server.TcpServer;

#endregion

namespace LobotomyCorporationMods.Playwright.Commands
{
    /// <summary>
    /// Debug command handlers - direct state manipulation.
    ///
    /// Expected game field names (via reflection):
    /// - AgentModel: hp, mental, fortitude, prudence, temperance, justice, instId, giftList, invincibility, targetCreature, workType, state, currentSefira
    /// - CreatureModel: instId, qliphothCounter, isSuppressed, state
    /// - PlayerModel: energy, energyMax
    /// - GameManager: speed, gameState, currentGameManager
    ///
    /// Note: These field names are based on Lobotomy Corporation game internals and may change with game updates.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class DebugCommands
    {
        /// <summary>
        /// Set agent stats: HP, mental, fortitude, prudence, temperance, justice.
        /// </summary>
        public static Response HandleSetAgentStats(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var paramsObj = GetParams<SetAgentStatsParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for set-agent-stats", "INVALID_PARAMS");
            }

            if (paramsObj.agentId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid agentId", "INVALID_AGENT_ID");
            }

            var agent = GetAgentById(paramsObj.agentId);
            if (agent == null)
            {
                return Response.CreateError(request.Id, $"Agent not found: {paramsObj.agentId}", "NOT_FOUND");
            }

            try
            {
                if (paramsObj.hp >= 0)
                {
                    SetPrivateField(agent, "hp", paramsObj.hp);
                }
                if (paramsObj.mental >= 0)
                {
                    SetPrivateField(agent, "mental", paramsObj.mental);
                }
                if (paramsObj.fortitude > 0)
                {
                    SetPrivateField(agent, "fortitude", paramsObj.fortitude);
                }
                if (paramsObj.prudence > 0)
                {
                    SetPrivateField(agent, "prudence", paramsObj.prudence);
                }
                if (paramsObj.temperance > 0)
                {
                    SetPrivateField(agent, "temperance", paramsObj.temperance);
                }
                if (paramsObj.justice > 0)
                {
                    SetPrivateField(agent, "justice", paramsObj.justice);
                }

                TcpServer.LogDebug($"[LobotomyPlaywright] Set agent {paramsObj.agentId} stats");
                return Response.CreateSuccess(request.Id, new { result = "stats_updated", paramsObj.agentId });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleSetAgentStats");
                return Response.CreateError(request.Id, $"Failed to set agent stats: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Add an E.G.O. gift to an agent.
        /// </summary>
        public static Response HandleAddGift(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var paramsObj = GetParams<GiftParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for add-gift", "INVALID_PARAMS");
            }

            if (paramsObj.agentId <= 0 || paramsObj.giftId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid agentId or giftId", "INVALID_PARAMS");
            }

            var agent = GetAgentById(paramsObj.agentId);
            if (agent == null)
            {
                return Response.CreateError(request.Id, $"Agent not found: {paramsObj.agentId}", "NOT_FOUND");
            }

            try
            {
                var giftList = GetPrivateField<List<int>>(agent, "giftList");
                if (giftList == null)
                {
                    giftList = new List<int>();
                    SetPrivateField(agent, "giftList", giftList);
                }

                if (!giftList.Contains(paramsObj.giftId))
                {
                    giftList.Add(paramsObj.giftId);
                    TcpServer.LogDebug($"[LobotomyPlaywright] Added gift {paramsObj.giftId} to agent {paramsObj.agentId}");
                }

                return Response.CreateSuccess(request.Id, new { result = "gift_added", paramsObj.agentId, paramsObj.giftId });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleAddGift");
                return Response.CreateError(request.Id, $"Failed to add gift: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Remove an E.G.O. gift from an agent.
        /// </summary>
        public static Response HandleRemoveGift(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var paramsObj = GetParams<GiftParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for remove-gift", "INVALID_PARAMS");
            }

            if (paramsObj.agentId <= 0 || paramsObj.giftId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid agentId or giftId", "INVALID_PARAMS");
            }

            var agent = GetAgentById(paramsObj.agentId);
            if (agent == null)
            {
                return Response.CreateError(request.Id, $"Agent not found: {paramsObj.agentId}", "NOT_FOUND");
            }

            try
            {
                var giftList = GetPrivateField<List<int>>(agent, "giftList");
                if (giftList != null && giftList.Contains(paramsObj.giftId))
                {
                    _ = giftList.Remove(paramsObj.giftId);
                    TcpServer.LogDebug($"[LobotomyPlaywright] Removed gift {paramsObj.giftId} from agent {paramsObj.agentId}");
                }

                return Response.CreateSuccess(request.Id, new { result = "gift_removed", paramsObj.agentId, paramsObj.giftId });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleRemoveGift");
                return Response.CreateError(request.Id, $"Failed to remove gift: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Set qliphoth counter on a creature.
        /// </summary>
        public static Response HandleSetQliphoth(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var paramsObj = GetParams<SetQliphothParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for set-qliphoth", "INVALID_PARAMS");
            }

            if (paramsObj.creatureId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid creatureId", "INVALID_CREATURE_ID");
            }

            var creature = GetCreatureById(paramsObj.creatureId);
            if (creature == null)
            {
                return Response.CreateError(request.Id, $"Creature not found: {paramsObj.creatureId}", "NOT_FOUND");
            }

            try
            {
                SetPrivateField(creature, "qliphothCounter", paramsObj.counter);
                TcpServer.LogDebug($"[LobotomyPlaywright] Set creature {paramsObj.creatureId} qliphoth to {paramsObj.counter}");
                return Response.CreateSuccess(request.Id, new { result = "qliphoth_set", paramsObj.creatureId, paramsObj.counter });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleSetQliphoth");
                return Response.CreateError(request.Id, $"Failed to set qliphoth: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Fill energy quota.
        /// </summary>
        public static Response HandleFillEnergy(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                var playerModel = GetPlayerModel();
                if (playerModel == null)
                {
                    return Response.CreateError(request.Id, "PlayerModel not available", "NOT_AVAILABLE");
                }

                var energy = GetPrivateField<float>(playerModel, "energy");
                var energyMax = GetPrivateField<float>(playerModel, "energyMax");

                SetPrivateField(playerModel, "energy", energyMax);
                TcpServer.LogDebug($"[LobotomyPlaywright] Filled energy to {energyMax}");
                return Response.CreateSuccess(request.Id, new { result = "energy_filled", energy = energyMax });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleFillEnergy");
                return Response.CreateError(request.Id, $"Failed to fill energy: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Set game speed.
        /// </summary>
        public static Response HandleSetGameSpeed(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var paramsObj = GetParams<SetGameSpeedParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for set-game-speed", "INVALID_PARAMS");
            }

            if (paramsObj.speed < 1 || paramsObj.speed > 5)
            {
                return Response.CreateError(request.Id, "Invalid speed (must be 1-5)", "INVALID_PARAMS");
            }

            try
            {
                var gameManager = GetGameManager();
                if (gameManager == null)
                {
                    return Response.CreateError(request.Id, "GameManager not available", "NOT_AVAILABLE");
                }

                SetPrivateField(gameManager, "speed", paramsObj.speed);
                TcpServer.LogDebug($"[LobotomyPlaywright] Set game speed to {paramsObj.speed}");
                return Response.CreateSuccess(request.Id, new { result = "speed_set", paramsObj.speed });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleSetGameSpeed");
                return Response.CreateError(request.Id, $"Failed to set game speed: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Set agent invincibility.
        /// </summary>
        public static Response HandleSetAgentInvincible(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var paramsObj = GetParams<SetAgentInvincibleParams>(request);
            if (paramsObj == null)
            {
                return Response.CreateError(request.Id, "Invalid parameters for set-agent-invincible", "INVALID_PARAMS");
            }

            if (paramsObj.agentId <= 0)
            {
                return Response.CreateError(request.Id, "Invalid agentId", "INVALID_AGENT_ID");
            }

            var agent = GetAgentById(paramsObj.agentId);
            if (agent == null)
            {
                return Response.CreateError(request.Id, $"Agent not found: {paramsObj.agentId}", "NOT_FOUND");
            }

            try
            {
                SetPrivateField(agent, "invincibility", paramsObj.invincible);
                TcpServer.LogDebug($"[LobotomyPlaywright] Set agent {paramsObj.agentId} invincible to {paramsObj.invincible}");
                return Response.CreateSuccess(request.Id, new { result = "invincibility_set", paramsObj.agentId, paramsObj.invincible });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleSetAgentInvincible");
                return Response.CreateError(request.Id, $"Failed to set invincibility: {ex.Message}", "COMMAND_ERROR");
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
                // Check if data is a string (JSON) or already a dictionary
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
                if (agentManager == null)
                {
                    return null;
                }

                var agentList = GetPrivateField<List<object>>(agentManager, "agentList");
                if (agentList == null)
                {
                    return null;
                }

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
                if (creatureManager == null)
                {
                    return null;
                }

                var creatureList = GetPrivateField<List<object>>(creatureManager, "creatureList");
                if (creatureList == null)
                {
                    return null;
                }

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

        private static object GetPlayerModel()
        {
            var type = typeof(PlayerModel);
            var field = type.GetField("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(null);
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
            if (value is T typedValue)
            {
                return typedValue;
            }

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
