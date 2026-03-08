// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyPlaywright.Protocol;
using UnityEngine;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Routes command requests to the appropriate handler.
    /// Phase 1.5: shutdown command only.
    /// Phase 3: Full command set.
    /// </summary>
    public static class CommandRouter
    {
        public static Protocol.Response HandleCommand(Protocol.Request request)
        {
            if (request == null)
            {
                return Protocol.Response.CreateError(
                    null,
                    "Request is null",
                    "INVALID_REQUEST"
                );
            }

            if (string.IsNullOrEmpty(request.Action))
            {
                return Protocol.Response.CreateError(
                    request.Id,
                    "Missing action",
                    "MISSING_ACTION"
                );
            }

            var action = request.Action.ToLowerInvariant();

            // Debug commands (state manipulation)
            switch (action)
            {
                case "shutdown":
                    return HandleShutdown(request);

                case "screenshot":
                    return ScreenshotHandler.HandleScreenshot(request);

                // Debug commands
                case "set-agent-stats":
                    return DebugCommands.HandleSetAgentStats(request);

                case "add-gift":
                    return DebugCommands.HandleAddGift(request);

                case "remove-gift":
                    return DebugCommands.HandleRemoveGift(request);

                case "set-qliphoth":
                    return DebugCommands.HandleSetQliphoth(request);

                case "fill-energy":
                    return DebugCommands.HandleFillEnergy(request);

                case "set-game-speed":
                    return DebugCommands.HandleSetGameSpeed(request);

                case "set-agent-invincible":
                    return DebugCommands.HandleSetAgentInvincible(request);

                // Player action simulation
                case "pause":
                    return PlayerActionCommands.HandlePause(request);

                case "unpause":
                    return PlayerActionCommands.HandleUnpause(request);

                case "assign-work":
                    return PlayerActionCommands.HandleAssignWork(request);

                case "deploy-agent":
                    return PlayerActionCommands.HandleDeployAgent(request);

                case "recall-agent":
                    return PlayerActionCommands.HandleRecallAgent(request);

                case "suppress":
                    return PlayerActionCommands.HandleSuppress(request);

                default:
                    return Protocol.Response.CreateError(
                        request.Id,
                        $"Unknown action: {request.Action}",
                        "UNKNOWN_ACTION"
                    );
            }
        }

        /// <summary>
        /// Handle the shutdown command - gracefully quit the game.
        /// </summary>
        // Excluded from code coverage - Unity runtime call
        // Note: net35 doesn't have ExcludeFromCodeCoverage attribute, handled via code analysis
        [SuppressMessage("Usage", "CA1806:Do not ignore method results", Justification = "Application.Quit has no return value to check")]
        private static Protocol.Response HandleShutdown(Protocol.Request request)
        {
            try
            {
                // Schedule Application.Quit on the next frame
                // This allows the response to be sent before quitting
                Plugin.Instance?.QueueShutdown();

                return Protocol.Response.CreateSuccess(
                    request.Id,
                    new { result = "shutdown scheduled" }
                );
            }
            catch (Exception ex)
            {
                return Protocol.Response.CreateError(
                    request.Id,
                    $"Failed to queue shutdown: {ex.Message}",
                    "SHUTDOWN_ERROR"
                );
            }
        }
    }
}
