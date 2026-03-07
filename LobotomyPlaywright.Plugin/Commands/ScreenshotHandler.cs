// SPDX-License-Identifier: MIT

using System;
using System.IO;
using UnityEngine;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Handles screenshot capture commands.
    /// Captures to current game screen and returns either a file path or base64-encoded image data.
    /// </summary>
    public static class ScreenshotHandler
    {
        /// <summary>
        /// Captures a screenshot of current game state.
        /// Note: This is a best-effort capture. Unity's ScreenCapture is async, so the file may not exist immediately.
        /// </summary>
        /// <param name="request">The request object containing parameters.</param>
        /// <returns>A response containing screenshot information.</returns>
        public static Protocol.Response HandleScreenshot(Protocol.Request request)
        {
            if (request == null)
            {
                return Protocol.Response.CreateError(
                    null,
                    "Request is null",
                    "INVALID_REQUEST"
                );
            }

            // Get format parameter (base64 or path), default to path for now
            string format = "path";
            if (request.Params != null && request.Params.ContainsKey("format"))
            {
                format = request.Params["format"].ToString().ToLowerInvariant();
            }

            // Generate unique filename with timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string filename = "loboplaywright_" + timestamp + ".png";

            try
            {
                // Capture screenshot using ScreenCapture
                // Saves to game's root directory (where the executable is)
                ScreenCapture.CaptureScreenshot(filename);

                // Build expected path - typically in the game's root directory
                string expectedPath = Path.Combine(Directory.GetCurrentDirectory(), filename);

                // Try to read the file immediately (it might not be ready yet)
                long fileSize = 0;
                if (File.Exists(expectedPath))
                {
                    byte[] imageBytes = File.ReadAllBytes(expectedPath);
                    fileSize = imageBytes.Length;
                }

                string timestampUtc = DateTime.UtcNow.ToString("O");

                // Use concrete class for proper JsonUtility serialization
                var data = new Protocol.ScreenshotData
                {
                    filename = filename,
                    path = expectedPath,
                    size = fileSize,
                    timestamp = timestampUtc,
                    format = "path",
                    note = "Screenshot capture initiated. File may not be ready immediately due to Unity's async capture."
                };

                return Protocol.Response.CreateSuccess(request.Id, data);
            }
            catch (Exception ex)
            {
                return Protocol.Response.CreateError(
                    request.Id,
                    "Failed to capture screenshot: " + ex.Message,
                    "SCREENSHOT_ERROR"
                );
            }
        }
    }
}
