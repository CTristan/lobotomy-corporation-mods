// SPDX-License-Identifier: MIT

using System;
using BepInEx;
using BepInEx.Logging;
using LobotomyPlaywright.Events;
using LobotomyPlaywright.Server;
using UnityEngine;

namespace LobotomyPlaywright
{
#pragma warning disable CA2243 // BepInEx uses reverse-domain IDs, not RFC GUID strings
    [BepInPlugin(PluginConstants.PluginGuid, PluginConstants.PluginName, PluginConstants.PluginVersion)]
#pragma warning restore CA2243
    public sealed class Plugin : BaseUnityPlugin
    {
        private static Plugin _instance;
        private PluginConfiguration _configuration;
        private TcpServer _tcpServer;
        private bool _shutdownQueued;

        internal static Plugin Instance => _instance;

        private void Awake()
        {
            try
            {
                _instance = this;

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
                var testResponse = Protocol.Response.CreateSuccess("init", new { test = true });
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
                Events.EventSubscriptionManager.TrySubscribeToEvents();
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

        private void OnDestroy()
        {
            if (_tcpServer != null)
            {
                _tcpServer.Stop();
                _tcpServer = null;
            }

            // Shutdown event streaming
            EventSubscriptionManager.Shutdown();

            LogInfo("LobotomyPlaywright TCP server stopped.");
        }
    }
}
