// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Threading;
using LobotomyPlaywright.JsonModels;
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
        public static Response HandleScreenshot(Request request)
        {
            if (request == null)
            {
                return Response.CreateError(
                    null,
                    "Request is null",
                    "INVALID_REQUEST"
                );
            }

            // Get format parameter (base64 or path), default to path
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

                // For base64 format, poll for the file to exist and read it
                string base64Data = null;
                long fileSize = 0;

                if (format == "base64")
                {
                    // Poll for the file to exist on a background thread to avoid blocking Unity main thread
                    byte[] imageBytes = null;
                    string base64Result = null;
                    long sizeResult = 0;

                    using (var doneEvent = new ManualResetEvent(false))
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try
                            {
                                int maxAttempts = 40; // 2 seconds total (40 * 50ms)
                                int attempt = 0;

                                while (attempt < maxAttempts)
                                {
                                    if (File.Exists(expectedPath))
                                    {
                                        try
                                        {
                                            imageBytes = File.ReadAllBytes(expectedPath);
                                            sizeResult = imageBytes.Length;
                                            base64Result = Convert.ToBase64String(imageBytes);
                                            break;
                                        }
                                        catch (IOException)
                                        {
                                            // File might be locked, retry
                                        }
                                    }
                                    attempt++;
                                    Thread.Sleep(50); // Poll every 50ms on background thread
                                }
                            }
                            finally
                            {
                                doneEvent.Set();
                            }
                        });

                        // Wait for background thread to complete (with timeout)
                        if (!doneEvent.WaitOne(3000)) // 3 second timeout (extra margin)
                        {
                            return Response.CreateError(
                                request.Id,
                                "Screenshot file not available after timeout",
                                "SCREENSHOT_TIMEOUT"
                            );
                        }
                    }

                    base64Data = base64Result;
                    fileSize = sizeResult;

                    if (base64Data == null)
                    {
                        return Response.CreateError(
                            request.Id,
                            "Screenshot file not available after 2 seconds",
                            "SCREENSHOT_TIMEOUT"
                        );
                    }
                }
                else
                {
                    // For path format, try to read the file immediately (it might not be ready yet)
                    if (File.Exists(expectedPath))
                    {
                        byte[] imageBytes = File.ReadAllBytes(expectedPath);
                        fileSize = imageBytes.Length;
                    }
                }

                string timestampUtc = DateTime.UtcNow.ToString("O");

                // Use concrete class for proper JsonUtility serialization
                var data = new ScreenshotData
                {
                    filename = filename,
                    path = expectedPath,
                    size = fileSize,
                    timestamp = timestampUtc,
                    format = format,
                    base64 = base64Data,
                    note = format == "base64"
                        ? "Base64-encoded screenshot data"
                        : "Screenshot capture initiated. File may not be ready immediately due to Unity's async capture."
                };

                return Response.CreateSuccess(request.Id, data);
            }
            catch (Exception ex)
            {
                return Response.CreateError(
                    request.Id,
                    "Failed to capture screenshot: " + ex.Message,
                    "SCREENSHOT_ERROR"
                );
            }
        }
    }
}
