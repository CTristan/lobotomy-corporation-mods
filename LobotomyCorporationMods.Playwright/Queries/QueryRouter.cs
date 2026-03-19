// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Hemocode.Playwright.JsonModels;

#endregion

namespace Hemocode.Playwright.Queries
{
    /// <summary>
    /// Routes query requests to the appropriate handler.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class QueryRouter
    {
        private static readonly HashSet<string> ValidTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "agents", "creatures", "game", "status", "sefira", "departments", "titlemenu", "title", "ui", "diagnostics", "gameobjects"
        };

        public static Response HandleQuery(Request request)
        {
            Server.TcpServer.LogDebug($"[LobotomyPlaywright] HandleQuery called: target={request?.Target}, id={request?.Id}");

            if (request == null || string.IsNullOrEmpty(request.Target))
            {
                return Response.CreateError(
                    request?.Id,
                    "Missing target parameter",
                    "MISSING_TARGET"
                );
            }

            try
            {
                var target = request.Target.ToLowerInvariant();

                if (!IsValidTarget(target))
                {
                    return Response.CreateError(
                        request.Id,
                        $"Unknown query target: {request.Target}",
                        "UNKNOWN_TARGET"
                    );
                }

                // Handle title menu queries (always available on title screen)
                if (IsTitleMenuTarget(target) && GameStateQueries.IsOnTitleScreen())
                {
                    return HandleTitleMenuQuery(request.Id);
                }

                // Diagnostics queries can be made at any time
                if (target == "diagnostics")
                {
                    var diagParams = request.Params ?? new Dictionary<string, object>();
                    return DiagnosticsQueries.HandleQuery(request.Id, diagParams);
                }

                // UI queries can be made at any time
                if (target == "ui")
                {
                    var uiParams = request.Params ?? new Dictionary<string, object>();
                    return HandleUiQuery(request.Id, uiParams);
                }

                // GameObject queries can be made at any time
                if (target == "gameobjects")
                {
                    var goParams = request.Params ?? new Dictionary<string, object>();
                    return HandleGameObjectsQuery(request.Id, goParams);
                }

                // All other queries require the game to be queryable
                if (!GameStateQueries.IsGameQueryable())
                {
                    Server.TcpServer.LogDebug("[LobotomyPlaywright] Game not queryable!");
                    return Response.CreateError(
                        request.Id,
                        "Game is not in a queryable state",
                        "GAME_NOT_READY"
                    );
                }

                var paramsDict = request.Params ?? new Dictionary<string, object>();
                Server.TcpServer.LogDebug($"[LobotomyPlaywright] Routing query: target={target}, isQueryable=true");

                return RouteQuery(target, request.Id, paramsDict);
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleQuery");
                return Response.CreateError(
                    request.Id,
                    $"Query failed: {ex.Message}",
                    "QUERY_ERROR"
                );
            }
        }

        private static bool IsValidTarget(string target)
        {
            return ValidTargets.Contains(target);
        }

        private static bool IsTitleMenuTarget(string target)
        {
            return target == "titlemenu" || target == "title";
        }

        private static Response RouteQuery(string target, string requestId, Dictionary<string, object> parameters)
        {
            switch (target)
            {
                case "agents":
                    return HandleAgentsQuery(requestId, parameters);
                case "creatures":
                    return HandleCreaturesQuery(requestId, parameters);
                case "game":
                case "status":
                    return HandleGameStatusQuery(requestId);
                case "sefira":
                case "departments":
                    return HandleSefiraQuery(requestId, parameters);
                case "titlemenu":
                case "title":
                    return HandleTitleMenuQuery(requestId);
                default:
                    return Response.CreateError(requestId, $"Unknown query target: {target}", "UNKNOWN_TARGET");
            }
        }

        private static long ExtractInstanceId(Dictionary<string, object> parameters)
        {
            var idKey = parameters.ContainsKey("id") ? "id" : "instanceId";
            var idValue = parameters[idKey];

            if (idValue is double)
            {
                return (long)(double)idValue;
            }

            return Convert.ToInt64(idValue);
        }

        private static Response HandleAgentsQuery(string requestId, Dictionary<string, object> parameters)
        {
            // Check if we're querying a specific agent
            if (parameters.ContainsKey("id") || parameters.ContainsKey("instanceId"))
            {
                var instanceId = ExtractInstanceId(parameters);

                var agent = AgentQueries.GetAgent(instanceId);
                if (agent == null)
                {
                    return Response.CreateError(
                        requestId,
                        $"Agent not found: {instanceId}",
                        "NOT_FOUND"
                    );
                }

                return Response.CreateSuccess(requestId, agent);
            }

            // List all agents
            var agents = AgentQueries.ListAgents();
            return Response.CreateSuccess(requestId, agents);
        }

        private static Response HandleCreaturesQuery(string requestId, Dictionary<string, object> parameters)
        {
            // Check if we're querying a specific creature
            if (parameters.ContainsKey("id") || parameters.ContainsKey("instanceId"))
            {
                var instanceId = ExtractInstanceId(parameters);

                var creature = CreatureQueries.GetCreature(instanceId);
                if (creature == null)
                {
                    return Response.CreateError(
                        requestId,
                        $"Creature not found: {instanceId}",
                        "NOT_FOUND"
                    );
                }

                return Response.CreateSuccess(requestId, creature);
            }

            // List all creatures
            var creatures = CreatureQueries.ListCreatures();
            return Response.CreateSuccess(requestId, creatures);
        }

        private static Response HandleGameStatusQuery(string requestId)
        {
            var status = GameStateQueries.GetStatus();
            return Response.CreateSuccess(requestId, status);
        }

        private static Response HandleSefiraQuery(string requestId, Dictionary<string, object> parameters)
        {
            // Check if we're querying a specific sefira
            if (parameters.ContainsKey("name") || parameters.ContainsKey("sefira"))
            {
                var sefiraName = parameters.ContainsKey("name")
                    ? parameters["name"].ToString()
                    : parameters["sefira"].ToString();

                try
                {
                    var sefiraEnum = (SefiraEnum)Enum.Parse(typeof(SefiraEnum), sefiraName, true);
                    var sefira = SefiraQueries.GetSefira(sefiraEnum);
                    return Response.CreateSuccess(requestId, sefira);
                }
                catch (ArgumentException)
                {
                    return Response.CreateError(
                        requestId,
                        $"Unknown sefira: {sefiraName}",
                        "NOT_FOUND"
                    );
                }
            }

            // List all sefira
            var sefiraList = SefiraQueries.ListSefira();
            return Response.CreateSuccess(requestId, sefiraList);
        }

        private static Response HandleTitleMenuQuery(string requestId)
        {
            try
            {
                var status = TitleMenuQueries.GetTitleMenuStatus();
                return Response.CreateSuccess(requestId, status);
            }
            catch (Exception ex)
            {
                return Response.CreateError(
                    requestId,
                    $"Failed to get title menu status: {ex.Message}",
                    "QUERY_ERROR"
                );
            }
        }

        private static Response HandleGameObjectsQuery(string requestId, Dictionary<string, object> parameters)
        {
            try
            {
                var mode = parameters.ContainsKey("mode")
                    ? parameters["mode"].ToString().ToLowerInvariant()
                    : "discover";

                if (mode == "inspect")
                {
                    return HandleGameObjectInspect(requestId, parameters);
                }

                if (mode == "search")
                {
                    return HandleGameObjectSearch(requestId, parameters);
                }

                return HandleGameObjectDiscover(requestId, parameters);
            }
            catch (Exception ex)
            {
                return Response.CreateError(
                    requestId,
                    $"Failed to query game objects: {ex.Message}",
                    "QUERY_ERROR"
                );
            }
        }

        private static Response HandleGameObjectInspect(string requestId, Dictionary<string, object> parameters)
        {
            var path = parameters.ContainsKey("path")
                ? parameters["path"].ToString()
                : null;

            if (string.IsNullOrEmpty(path))
            {
                return Response.CreateError(requestId, "Missing 'path' parameter for inspect mode", "MISSING_PARAM");
            }

            var detail = parameters.ContainsKey("detail")
                ? parameters["detail"].ToString()
                : "summary";

            var inspectData = GameObjectQueries.Inspect(path, detail);
            if (inspectData == null)
            {
                return Response.CreateError(requestId, $"GameObject not found: {path}", "NOT_FOUND");
            }

            return Response.CreateSuccess(requestId, inspectData);
        }

        private static Response HandleGameObjectSearch(string requestId, Dictionary<string, object> parameters)
        {
            var name = parameters.ContainsKey("name")
                ? parameters["name"].ToString()
                : null;

            var nameMatch = parameters.ContainsKey("nameMatch")
                ? parameters["nameMatch"].ToString()
                : "contains";

            var component = parameters.ContainsKey("component")
                ? parameters["component"].ToString()
                : null;

            var tag = parameters.ContainsKey("tag")
                ? parameters["tag"].ToString()
                : null;

            bool? activeOnly = parameters.ContainsKey("activeOnly") &&
                               parameters["activeOnly"] is bool activeOnlyVal
                ? (bool?)activeOnlyVal
                : null;

            var searchResult = GameObjectQueries.Search(name, nameMatch, component, tag, activeOnly);
            return Response.CreateSuccess(requestId, searchResult);
        }

        private static Response HandleGameObjectDiscover(string requestId, Dictionary<string, object> parameters)
        {
            var depth = parameters.ContainsKey("depth") && parameters["depth"] is double depthVal
                ? (int)depthVal
                : 3;

            var dumpPath = parameters.ContainsKey("dump") && parameters["dump"] is bool dumpVal && dumpVal
                ? Path.Combine(
                    Path.Combine(Path.GetDirectoryName(UnityEngine.Application.dataPath), ".lobotomy-playwright"),
                    "gameobject_dump.json")
                : null;

            var roots = GameObjectQueries.Discover(depth, dumpPath);
            return Response.CreateSuccess(requestId, new { roots, rootCount = roots.Count });
        }

        private static Response HandleUiQuery(string requestId, Dictionary<string, object> parameters)
        {
            try
            {
                // UI queries can be made at any time (not just when game is queryable)
                var depth = parameters.ContainsKey("depth")
                    ? parameters["depth"].ToString()
                    : "full";

                var windowFilter = parameters.ContainsKey("name")
                    ? parameters["name"].ToString()
                    : null;

                var uiState = UiQueries.GetUiState(depth, windowFilter);
                return Response.CreateSuccess(requestId, uiState);
            }
            catch (Exception ex)
            {
                return Response.CreateError(
                    requestId,
                    $"Failed to get UI state: {ex.Message}",
                    "QUERY_ERROR"
                );
            }
        }
    }
}
