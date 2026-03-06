// SPDX-License-Identifier: MIT

using BepInEx;
using BepInEx.Logging;
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

        internal static Plugin Instance => _instance;

        private void Awake()
        {
            _instance = this;
            _configuration = PluginConfiguration.Bind(Config);

            if (!_configuration.Enabled.Value)
            {
                Logger.LogInfo("LobotomyPlaywright is disabled via configuration.");
                return;
            }

            // Force MessageSerializer to initialize
            var testResponse = Protocol.Response.CreateSuccess("init", new { test = true });
            var testJson = Protocol.MessageSerializer.Serialize(testResponse);
            Logger.LogInfo($"MessageSerializer test: {testJson}");

            try
            {
                _tcpServer = new TcpServer(_configuration.Port.Value);
                _tcpServer.Start();
                Logger.LogInfo($"LobotomyPlaywright v{PluginConstants.PluginVersion} initialized.");
                Logger.LogInfo($"TCP server listening on 127.0.0.1:{_configuration.Port.Value}");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Failed to start TCP server: {ex.Message}");
            }
        }

        private void Update()
        {
            if (_tcpServer != null && _tcpServer.IsRunning)
            {
                _tcpServer.ProcessQueuedRequests();

                // Debug: Log current scene once per second
                if (Time.frameCount % 60 == 0)
                {
                    try
                    {
                        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                        Logger.LogInfo($"Current scene: {sceneName}, clientCount={_tcpServer.ClientCount}");
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogWarning($"Could not get scene name: {ex.Message}");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_tcpServer != null)
            {
                _tcpServer.Stop();
                _tcpServer = null;
                Logger.LogInfo("LobotomyPlaywright TCP server stopped.");
            }
        }
    }
}
