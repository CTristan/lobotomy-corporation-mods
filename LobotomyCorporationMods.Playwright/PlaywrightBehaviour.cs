// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Playwright.Events;
using LobotomyCorporationMods.Playwright.JsonModels;
using LobotomyCorporationMods.Playwright.Server;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Playwright
{
    /// <summary>
    /// MonoBehaviour that manages the TCP server lifecycle.
    /// Attached to a DontDestroyOnLoad GameObject by PlaywrightCore.Initialize.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class PlaywrightBehaviour : MonoBehaviour
    {
        private TcpServer _tcpServer;
        private bool _shutdownQueued;

        /// <summary>
        /// Initializes the TCP server and event streaming.
        /// Called by PlaywrightCore.Initialize after AddComponent.
        /// </summary>
        /// <param name="port">TCP port to listen on.</param>
        public void Initialize(int port)
        {
            // Force MessageSerializer to initialize
            var testResponse = Response.CreateSuccess("init", new { test = true });
            var testJson = Protocol.MessageSerializer.Serialize(testResponse);
            Debug.Log("[LobotomyPlaywright] MessageSerializer test: " + testJson);

            _tcpServer = new TcpServer(port);
            _tcpServer.Start();

            // Initialize event streaming
            EventSubscriptionManager.Initialize(_tcpServer);

            Debug.Log("[LobotomyPlaywright] v" + PluginConstants.PluginVersion + " initialized.");
            Debug.Log("[LobotomyPlaywright] TCP server listening on 127.0.0.1:" + port);
        }

        /// <summary>
        /// Queues a shutdown to be processed on the main thread.
        /// </summary>
        public void QueueShutdown()
        {
            _shutdownQueued = true;
            Debug.Log("[LobotomyPlaywright] Shutdown queued.");
        }

        private void Update()
        {
            // Process shutdown if queued
            if (_shutdownQueued)
            {
                _shutdownQueued = false;
                Debug.Log("[LobotomyPlaywright] Processing queued shutdown...");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            if (_tcpServer != null && _tcpServer.IsRunning)
            {
                _tcpServer.ProcessQueuedRequests();

                // Try to subscribe to events if not already done
                EventSubscriptionManager.TrySubscribeToEvents();
            }
        }

        private void OnDestroy()
        {
            _tcpServer?.Stop();
            _tcpServer = null;

            // Shutdown event streaming
            EventSubscriptionManager.Shutdown();

            Debug.Log("[LobotomyPlaywright] TCP server stopped.");
        }
    }
}
