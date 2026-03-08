// SPDX-License-Identifier: MIT

using System;
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

            switch (request.Action.ToLowerInvariant())
            {
                case "shutdown":
                    return HandleShutdown(request);

                case "screenshot":
                    return ScreenshotHandler.HandleScreenshot(request);

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
        // Pragma warning disable needed because Unity's Application.Quit has no effect in tests
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
