// SPDX-License-Identifier: MIT

using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace LobotomyPlaywright.Infrastructure
{
    /// <summary>
    /// Helper methods for handling exceptions in the LobotomyPlaywright CLI.
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>
        /// Logs an exception to stderr and attempts to send a shutdown command to the game via TCP.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="context">Context information about where the error occurred.</param>
        /// <param name="host">The game TCP server host (default: localhost).</param>
        /// <param name="port">The game TCP server port (default: 8484).</param>
        public static void LogAndAttemptShutdown(Exception ex, string context, string? host = null, int port = 8484)
        {
            var message = $"[LobotomyPlaywright CLI] FATAL [{context}]: {ex}";
            Console.Error.WriteLine(message);

            if (string.IsNullOrEmpty(host))
            {
                host = "localhost";
            }

            // Best-effort attempt to send shutdown command to the game
            TrySendShutdown(host, port);
        }

        /// <summary>
        /// Attempts to send a shutdown command to the game via TCP.
        /// </summary>
        /// <param name="host">The game TCP server host.</param>
        /// <param name="port">The game TCP server port.</param>
        private static void TrySendShutdown(string host, int port)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var timeoutTask = System.Threading.Tasks.Task.Delay(3000); // 3 second timeout

                if (System.Threading.Tasks.Task.WhenAny(connectTask, timeoutTask).Result == timeoutTask)
                {
                    // Connection timeout - game may already be dead
                    return;
                }

                if (!client.Connected)
                {
                    return;
                }

                var shutdownCommand = JsonSerializer.Serialize(new { type = "command", action = "shutdown" });
                var bytes = Encoding.UTF8.GetBytes(shutdownCommand + "\n");

                var stream = client.GetStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
            catch
            {
                // Silently ignore shutdown failures - we're already in an error state
            }
        }
    }
}
