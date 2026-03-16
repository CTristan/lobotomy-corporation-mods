// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Playwright.Server;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Playwright
{
    /// <summary>
    /// Static entry point for LobotomyPlaywright initialization.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PlaywrightCore
    {
        /// <summary>
        /// Gets the PlaywrightBehaviour instance, if initialized.
        /// </summary>
        public static PlaywrightBehaviour Instance { get; private set; }

        /// <summary>
        /// Gets a value indicating whether PlaywrightCore has been initialized.
        /// </summary>
        public static bool IsInitialized => Instance != null;

        /// <summary>
        /// Initializes the Playwright TCP server on a persistent GameObject.
        /// No-op if already initialized.
        /// </summary>
        /// <param name="port">TCP port to listen on.</param>
        public static void Initialize(int port)
        {
            if (IsInitialized)
            {
                return;
            }

            var go = new GameObject("LobotomyPlaywright");
            UnityEngine.Object.DontDestroyOnLoad(go);

            Instance = go.AddComponent<PlaywrightBehaviour>();
            Instance.Initialize(port);
        }

        /// <summary>
        /// Handles a fatal exception by logging it and queueing a game shutdown.
        /// </summary>
        public static void HandleFatalException(Exception ex, string context)
        {
            var message = "[LobotomyPlaywright] FATAL [" + context + "]: " + ex;
            TcpServer.LogError(message);
            Instance?.QueueShutdown();
        }
    }
}
