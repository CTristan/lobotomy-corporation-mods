// SPDX-License-Identifier: MIT

using BepInEx;
using LobotomyPlaywright.Events;
using LobotomyPlaywright.JsonModels;
using LobotomyPlaywright.Server;
using UnityEngine;

namespace LobotomyPlaywright
{
    [BepInPlugin(PluginConstants.PluginGuid, PluginConstants.PluginName, PluginConstants.PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private PluginConfiguration _configuration;
        private TcpServer _tcpServer;
        private bool _shutdownQueued;

        internal static Plugin Instance { get; private set; }

        private void Awake()
        {
            try
            {
                Instance = this;

                if (Config == null)
                {
                    LogWarning("LobotomyPlaywright: BepInEx Config is null (constructor bug?). Using defaults.");
                }

                _configuration = PluginConfiguration.Bind(Config);

                if (!_configuration.Enabled)
                {
                    LogInfo("LobotomyPlaywright is disabled via configuration.");
                    return;
                }

                // Force MessageSerializer to initialize
                var testResponse = Response.CreateSuccess("init", new { test = true });
                var testJson = Protocol.MessageSerializer.Serialize(testResponse);
                LogInfo($"MessageSerializer test: {testJson}");

                _tcpServer = new TcpServer(_configuration.Port);
                _tcpServer.Start();

                // Initialize event streaming
                EventSubscriptionManager.Initialize(_tcpServer);

                LogInfo($"LobotomyPlaywright v{PluginConstants.PluginVersion} initialized.");
                LogInfo($"TCP server listening on 127.0.0.1:{_configuration.Port}");
            }
            catch (System.Exception ex)
            {
                HandleFatalException(ex, "Awake");
                LogError($"LobotomyPlaywright initialization error: {ex.Message}");
            }
        }

        private void LogInfo(string message)
        {
            if (Logger != null)
            {
                Logger.LogInfo(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            if (Logger != null)
            {
                Logger.LogWarning(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
        }

        private void LogError(string message)
        {
            if (Logger != null)
            {
                Logger.LogError(message);
            }
            else
            {
                Debug.LogError(message);
            }
        }

        private void Update()
        {
            // Process shutdown if queued
            if (_shutdownQueued)
            {
                _shutdownQueued = false;
                LogInfo("LobotomyPlaywright: Processing queued shutdown...");
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

        /// <summary>
        /// Queues a shutdown to be processed on the main thread.
        /// </summary>
        public void QueueShutdown()
        {
            _shutdownQueued = true;
            LogInfo("LobotomyPlaywright: Shutdown queued.");
        }

        /// <summary>
        /// Handles a fatal exception by logging it and queueing a game shutdown.
        /// </summary>
        internal static void HandleFatalException(System.Exception ex, string context)
        {
            var message = "[LobotomyPlaywright] FATAL [" + context + "]: " + ex;
            TcpServer.LogError(message);
            Instance?.QueueShutdown();
        }

        private void OnDestroy()
        {
            _tcpServer?.Stop();
            _tcpServer = null;

            // Shutdown event streaming
            EventSubscriptionManager.Shutdown();

            LogInfo("LobotomyPlaywright TCP server stopped.");
        }
    }
}
