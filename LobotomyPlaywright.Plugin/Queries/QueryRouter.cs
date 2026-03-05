// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LobotomyPlaywright.Queries
{
    /// <summary>
    /// Routes query requests to the appropriate handler.
    /// </summary>
    public static class QueryRouter
    {
        public static Protocol.Response HandleQuery(Protocol.Request request)
        {
            if (request == null || string.IsNullOrEmpty(request.Target))
            {
                return Protocol.Response.CreateError(
                    request?.Id,
                    "Missing target parameter",
                    "MISSING_TARGET"
                );
            }

            try
            {
                if (!GameStateQueries.IsGameQueryable())
                {
                    return Protocol.Response.CreateError(
                        request.Id,
                        "Game is not in a queryable state",
                        "GAME_NOT_READY"
                    );
                }

                var target = request.Target.ToLowerInvariant();
                var paramsDict = request.Params ?? new Dictionary<string, object>();

                switch (target)
                {
                    case "agents":
                        return HandleAgentsQuery(request.Id, paramsDict);

                    case "creatures":
                        return HandleCreaturesQuery(request.Id, paramsDict);

                    case "game":
                    case "status":
                        return HandleGameStatusQuery(request.Id);

                    case "sefira":
                    case "departments":
                        return HandleSefiraQuery(request.Id, paramsDict);

                    default:
                        return Protocol.Response.CreateError(
                            request.Id,
                            $"Unknown query target: {request.Target}",
                            "UNKNOWN_TARGET"
                        );
                }
            }
            catch (Exception ex)
            {
                return Protocol.Response.CreateError(
                    request.Id,
                    $"Query failed: {ex.Message}",
                    "QUERY_ERROR"
                );
            }
        }

        private static Protocol.Response HandleAgentsQuery(string requestId, Dictionary<string, object> parameters)
        {
            // Check if we're querying a specific agent
            if (parameters.ContainsKey("id") || parameters.ContainsKey("instanceId"))
            {
                long instanceId;
                if (parameters.ContainsKey("id"))
                {
                    var idValue = parameters["id"];
                    if (idValue is double d)
                    {
                        instanceId = (long)d;
                    }
                    else
                    {
                        instanceId = Convert.ToInt64(idValue);
                    }
                }
                else
                {
                    var idValue = parameters["instanceId"];
                    if (idValue is double d)
                    {
                        instanceId = (long)d;
                    }
                    else
                    {
                        instanceId = Convert.ToInt64(idValue);
                    }
                }

                var agent = AgentQueries.GetAgent(instanceId);
                if (agent == null)
                {
                    return Protocol.Response.CreateError(
                        requestId,
                        $"Agent not found: {instanceId}",
                        "NOT_FOUND"
                    );
                }

                return Protocol.Response.CreateSuccess(requestId, agent);
            }

            // List all agents
            var agents = AgentQueries.ListAgents();
            return Protocol.Response.CreateSuccess(requestId, agents);
        }

        private static Protocol.Response HandleCreaturesQuery(string requestId, Dictionary<string, object> parameters)
        {
            // Check if we're querying a specific creature
            if (parameters.ContainsKey("id") || parameters.ContainsKey("instanceId"))
            {
                long instanceId;
                if (parameters.ContainsKey("id"))
                {
                    var idValue = parameters["id"];
                    if (idValue is double d)
                    {
                        instanceId = (long)d;
                    }
                    else
                    {
                        instanceId = Convert.ToInt64(idValue);
                    }
                }
                else
                {
                    var idValue = parameters["instanceId"];
                    if (idValue is double d)
                    {
                        instanceId = (long)d;
                    }
                    else
                    {
                        instanceId = Convert.ToInt64(idValue);
                    }
                }

                var creature = CreatureQueries.GetCreature(instanceId);
                if (creature == null)
                {
                    return Protocol.Response.CreateError(
                        requestId,
                        $"Creature not found: {instanceId}",
                        "NOT_FOUND"
                    );
                }

                return Protocol.Response.CreateSuccess(requestId, creature);
            }

            // List all creatures
            var creatures = CreatureQueries.ListCreatures();
            return Protocol.Response.CreateSuccess(requestId, creatures);
        }

        private static Protocol.Response HandleGameStatusQuery(string requestId)
        {
            var status = GameStateQueries.GetStatus();
            return Protocol.Response.CreateSuccess(requestId, status);
        }

        private static Protocol.Response HandleSefiraQuery(string requestId, Dictionary<string, object> parameters)
        {
            // Check if we're querying a specific sefira
            if (parameters.ContainsKey("name") || parameters.ContainsKey("sefira"))
            {
                string sefiraName = parameters.ContainsKey("name")
                    ? parameters["name"].ToString()
                    : parameters["sefira"].ToString();

                try
                {
                    var sefiraEnum = (SefiraEnum)Enum.Parse(typeof(SefiraEnum), sefiraName, true);
                    var sefira = SefiraQueries.GetSefira(sefiraEnum);
                    return Protocol.Response.CreateSuccess(requestId, sefira);
                }
                catch (ArgumentException)
                {
                    return Protocol.Response.CreateError(
                        requestId,
                        $"Unknown sefira: {sefiraName}",
                        "NOT_FOUND"
                    );
                }
            }

            // List all sefira
            var sefiraList = SefiraQueries.ListSefira();
            return Protocol.Response.CreateSuccess(requestId, sefiraList);
        }
    }
}
